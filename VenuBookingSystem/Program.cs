// See https://aka.ms/new-console-template for more information

// Define your booking parameters
TimeSpan minTimeSlot = TimeSpan.FromMinutes(30);
TimeSpan bufferTime = TimeSpan.FromMinutes(30);

TimeOnly startTime = new(5,0);
TimeOnly endTime = new(20,0);
var nl = Environment.NewLine;

// Simulated booked slots This will come from DB 

List<TimeSlot> bookedSlots = new() {
    new TimeSlot { StartTime = new(5,30), EndTime = new(7,30) },
    new TimeSlot { StartTime = new(9,00), EndTime = new(10,00) },
    new TimeSlot { StartTime = new(12,00), EndTime = new(13,00) },
    new TimeSlot { StartTime = new(14,30), EndTime = new(16,30) },
    new TimeSlot { StartTime = new(18,00), EndTime = new(19,00) },
}; 


Console.WriteLine($"Min Start Intervals: {minTimeSlot}, bufferTime : {bufferTime}, startTime : {startTime}, endTime: {endTime}" + nl);


// Generate all possible start and end Times within the specified time range
var allPossibleStartTimes = GetAllPossibleStartTimes(startTime, endTime, minTimeSlot);

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

// Generate available slots
List<TimeSlot> availableSlots = GetAvailableSlots(allPossibleStartTimes, bufferTime, bookedSlots);

Console.WriteLine(nl + $"Remaining Available Slots : {availableSlots.Count}" + nl);

// Print available slots
foreach (var slot in availableSlots)
{
    Console.WriteLine($"Available Slot: Slot #: {slot.SlotNumber}, {slot.StartTime} - {slot.EndTime}");
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

    List<TimeSlot> availableSlots = new();

    //Filter # 1 Iterate through booked slots to remove any overlaps
    foreach (var cSlot in allPossibleStartTimes)
    {
        bool isOverlap = false;

        foreach (var bookedSlot in bookedSlots)
        {
            // Check if the available slot overlaps with the booked slot (with buffer)
            if ((cSlot.StartTime >= bookedSlot.StartTime.AddMinutes(- bufferTime.Minutes) && cSlot.StartTime < bookedSlot.EndTime.AddMinutes(bufferTime.Minutes)) ||
                (cSlot.EndTime > bookedSlot.StartTime.AddMinutes(-bufferTime.Minutes) && cSlot.EndTime <= bookedSlot.EndTime.AddMinutes(bufferTime.Minutes)))
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

    //Console.WriteLine(nl + $"*******Filter #1 After Removing Overlaps********" + nl );
    foreach (var slot in availableSlots)
    {
        //Console.WriteLine($"Available Slot: Slot #: {slot.SlotNumber}, {slot.Start} - {slot.End}");
    }

    var partialSlots = GetPartialSlots(availableSlots);

    HashSet<int> partialSlotNumbers = new HashSet<int>(partialSlots.Select(slot => slot.SlotNumber));

    // Use DeleteRange to remove items in availableSlots where SlotNumber is not present in allSlots
    availableSlots.RemoveAll(slot => partialSlotNumbers.Contains(slot.SlotNumber));

    //Console.WriteLine(nl + $"*******Filter #2 After Removing Partial Slots********" + nl );
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


Console.ReadLine();