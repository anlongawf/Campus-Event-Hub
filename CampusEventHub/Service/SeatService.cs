using CampusEventHub.Models;

namespace CampusEventHub.Service;

public class SeatService
{
    private List<Seat> GenerateSeats(char allowedMaxRow, int seatsPerRow = 20)
    {
        var seats = new List<Seat>();
        char maxRow = 'F'; // luôn tạo từ A–F

        for (char row = 'A'; row <= maxRow; row++)
        {
            for (int number = 1; number <= seatsPerRow; number++)
            {
                var seat = new Seat
                {
                    Row = row,
                    SeatNumber = number,
                    Status = row <= allowedMaxRow ? SeatStatus.Available : SeatStatus.Reserved
                };
                seats.Add(seat);
            }
        }

        return seats;
    }

    // A–B trống, còn lại khóa
    public List<Seat> GenerateSeatsForClass(int seatsPerRow = 20)
    {
        return GenerateSeats('B', seatsPerRow);
    }

    // A–D trống, E–F khóa
    public List<Seat> GenerateSeatsForCourse(int seatsPerRow = 20)
    {
        return GenerateSeats('D', seatsPerRow);
    }

    // A–F đều trống
    public List<Seat> GenerateSeatsForToanDepartment(int seatsPerRow = 20)
    {
        return GenerateSeats('F', seatsPerRow);
    }
}
