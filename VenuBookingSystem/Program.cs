// See https://aka.ms/new-console-template for more information

// Define your booking parameters
TimeSpan minStartTimeIncrement = TimeSpan.FromMinutes(30); // This allows us to start at 00 or 30 hours like 10:00 AM or 10:30 AM after buffer 
TimeSpan bufferTime = TimeSpan.FromMinutes(30);
TimeSpan minBookableSlot = TimeSpan.FromMinutes(60); // A min 1 hour / 60 min slot must be booked. 

TimeOnly startTime = new(5, 0);
TimeOnly endTime = new(22, 0);
var nl = Environment.NewLine;

// Simulated booked slots This will come from DB 

List<TimeSlot> blackedoutSlots = new()
{
    new TimeSlot { StartTime = new(10, 30), EndTime = new(11, 30) },
    new TimeSlot { StartTime = new(19, 30), EndTime = new(22, 30) },
};

List<TimeSlot> bookedSlots = new()
{
    //new TimeSlot { StartTime = new(5, 30), EndTime = new(7, 30) },
    //new TimeSlot { StartTime = new(9, 00), EndTime = new(10, 00) },
    //new TimeSlot { StartTime = new(16, 00), EndTime = new(17, 00) },
    //new TimeSlot { StartTime = new(14,30), EndTime = new(16,30) },
    //new TimeSlot { StartTime = new(18,00), EndTime = new(19,00) },
};


Console.WriteLine($"Min Start Intervals: {minStartTimeIncrement}, bufferTime : {bufferTime}, startTime : {startTime}, endTime: {endTime}" + nl);


// Generate all possible start and end Times within the specified time range
var allPossibleStartTimes = GetAllPossibleStartTimes(startTime, endTime, minStartTimeIncrement);

Console.WriteLine($"*** ALL Possible start and end times between {startTime} - {endTime}" + nl);

foreach (var als in allPossibleStartTimes)
{
    Console.WriteLine($"Slot #: {als.SlotNumber}  {als.StartTime} - {als.EndTime}");
}

Console.WriteLine(nl + "*****Booked Slots******" + nl);

foreach (var bs in bookedSlots)
{
    Console.WriteLine($"Booked from {bs.StartTime} to {bs.EndTime}, Hours {bs.EndTime - bs.StartTime}");
}


Console.WriteLine(nl + "*****Blacked out Slots******" + nl);

foreach (var bs in blackedoutSlots)
{
    Console.WriteLine($"Blacked out from {bs.StartTime} to {bs.EndTime}, Hours {bs.EndTime - bs.StartTime}");
}

// Generate available slots
List<TimeSlot> availableSlots = GetAvailableSlots(allPossibleStartTimes, bufferTime, bookedSlots);


Console.WriteLine(nl + "*****BEFORE removing Blacked out Slots******" + nl);

foreach (var bs in availableSlots)
{
    Console.WriteLine($"Available from {bs.StartTime} to {bs.EndTime}, Hours {bs.EndTime - bs.StartTime}");
}

//Remove Blocked Out slots 
availableSlots = RemoveOverlappedSlots(availableSlots, bufferTime, blackedoutSlots);

Console.WriteLine(nl + $"FINAL Remaining Available Slots : {availableSlots.Count}" + nl);

// Print available slots
foreach (var slot in availableSlots)
{
    Console.WriteLine($"Available Slot: Slot #: {slot.SlotNumber}, {slot.StartTime} - {slot.EndTime}");
}

foreach (var slot in availableSlots)
{
    Console.WriteLine($"For {slot.StartTime} .. Possible End times are ...");

    var possibleEndTimes = GetPossibleEndTimes(slot.StartTime, minBookableSlot, availableSlots);

    foreach (var endtime in possibleEndTimes)
    {
        Console.WriteLine($"{endtime}");
    }


}

static List<TimeSlot> GetAllPossibleStartTimes(TimeOnly startTime, TimeOnly endTime, TimeSpan minTimeSlot)
{
    // Generate all possible time slots within the specified time range
    List<TimeSlot> allPossibleSlots = new();

    TimeOnly currentTime = startTime;
    int slotNumber = 1;
    while (currentTime < endTime)
    {
        allPossibleSlots.Add(new() { SlotNumber = slotNumber, StartTime = currentTime, EndTime = currentTime.AddMinutes(minTimeSlot.Minutes) });
        currentTime = currentTime.AddMinutes(minTimeSlot.Minutes);
        slotNumber++;
    }

    return allPossibleSlots;
}

static List<TimeSlot> GetAvailableSlots(List<TimeSlot> allPossibleStartTimes, TimeSpan bufferTime, List<TimeSlot> bookedSlots)
{
    var nl = Environment.NewLine;

    List<TimeSlot> availableSlots = RemoveOverlappedSlots(allPossibleStartTimes, bufferTime, bookedSlots);

    //Console.WriteLine(nl + $"*******Filter #1 After Removing Overlaps********" + nl );
    foreach (var slot in availableSlots)
    {
        //Console.WriteLine($"Available Slot: Slot #: {slot.SlotNumber}, {slot.Start} - {slot.End}");
    }

    var partialSlots = GetPartialSlots(availableSlots);

    Console.WriteLine(nl + $"******* Partial / half slots (1 slot = min 1 hr) ********" + nl);

    foreach (var slot in partialSlots)
    {
        Console.WriteLine($"Partial Slot: Slot #: {slot.SlotNumber}, {slot.StartTime} - {slot.EndTime}");
    }

    List<int> partialSlotNumbers = new List<int>(partialSlots.Select(slot => slot.SlotNumber));

    // Use DeleteRange to remove items in availableSlots where SlotNumber is not present in allSlots
    availableSlots.RemoveAll(slot => partialSlotNumbers.Contains(slot.SlotNumber));

    //Console.WriteLine(nl + $"*******Filter #2 After Removing Partial Slots********" + nl );
    return availableSlots;
}

static List<TimeSlot> RemoveOverlappedSlots(List<TimeSlot> allPossibleStartTimes, TimeSpan bufferTime, List<TimeSlot> slotsTobeRemoved)
{
    List<TimeSlot> availableSlots = new();

    //Filter # 1 Iterate through booked slots to remove any overlaps
    foreach (var cSlot in allPossibleStartTimes)
    {
        bool isOverlap = false;

        foreach (var remSlot in slotsTobeRemoved)
        {
            // Check if the available slot overlaps with the booked slot (with buffer)
            if ((cSlot.StartTime >= remSlot.StartTime.AddMinutes(-bufferTime.Minutes) && cSlot.StartTime < remSlot.EndTime.AddMinutes(bufferTime.Minutes)) ||
                (cSlot.EndTime > remSlot.StartTime.AddMinutes(-bufferTime.Minutes) && cSlot.EndTime <= remSlot.EndTime.AddMinutes(bufferTime.Minutes)))
            {
                isOverlap = true;
                break; // No need to check further, it overlaps
            }
        }

        // If there is no overlap, add the available slot to the new list
        if (!isOverlap)
        {
            availableSlots.Add(cSlot);
        }
    }

    return availableSlots;
}

static List<TimeSlot> GetPartialSlots(List<TimeSlot> slots)
{
    List<TimeSlot> partialSlots = new();

    for (int i = 0; i < slots.Count; i++)
    {
        // Check if the slot number before or after the current index
        // is not exactly 1 less or 1 more than the current slot number.
        if ((i == 0 || slots[i].SlotNumber - slots[i - 1].SlotNumber != 1) &&
            (i == slots.Count - 1 || slots[i + 1].SlotNumber - slots[i].SlotNumber != 1))
        {
            partialSlots.Add(slots[i]);
        }
    }
    return partialSlots;
}

static List<TimeOnly> GetPossibleEndTimes(TimeOnly startTime, TimeSpan minBookableSlot, List<TimeSlot> availableSlots)
{
    List<TimeOnly> possibleEndTimes = new List<TimeOnly>();

    var potentialEndTime = startTime.AddMinutes(minBookableSlot.Minutes); //If start is 12 potential Endtime is 1 

    var lastSlotEndTime = potentialEndTime;

    var lastPossibleEndTime = availableSlots.LastOrDefault()?.EndTime;

    while (potentialEndTime <= lastPossibleEndTime)
    {
        //Check if this endtime is in AvailableSlot 
        var endTimeExists = availableSlots.Where(s => s.EndTime == potentialEndTime).Count();

        //If the time difference between last aded slot and new slot is NOT EQ minBookableSlot then that means in between slots are booked. 
        if (endTimeExists > 0 && ((potentialEndTime != startTime && potentialEndTime == lastSlotEndTime) || potentialEndTime - lastSlotEndTime == minBookableSlot))
        {
            possibleEndTimes.Add(potentialEndTime);

            lastSlotEndTime = potentialEndTime;
        }



        potentialEndTime = potentialEndTime.AddHours(1);
    }

    return possibleEndTimes;
}

Console.ReadLine();