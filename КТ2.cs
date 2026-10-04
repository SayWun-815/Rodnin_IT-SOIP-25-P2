using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBookingSystem
{
    public class Movie
    {
        public string Title { get; }
        public int DurationMinutes { get; }

        public Movie(string title, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название фильма не может быть пустым");
            if (durationMinutes <= 0)
                throw new ArgumentException("Длительность должна быть положительной");

            Title = title;
            DurationMinutes = durationMinutes;
        }

        public override string ToString() => $"\"{Title}\" ({DurationMinutes} мин.)";
    }
    public class Seat
    {
        public int Number { get; }
        public Movie Movie { get; }
        public bool IsReserved { get; private set; }

        public Seat(int number, Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));

            Number = number;
            Movie = movie;
            IsReserved = false;
        }

        public void Reserve()
        {
            if (IsReserved)
                throw new InvalidOperationException(
                    $"Место №{Number} на фильм {Movie.Title} уже забронировано!");

            IsReserved = true;
        }

        public void CancelReservation()
        {
            if (!IsReserved)
                throw new InvalidOperationException(
                    $"Место №{Number} на фильм {Movie.Title} не забронировано!");

            IsReserved = false;
        }

        public override string ToString() =>
            $"Место №{Number} ({Movie.Title}) [{(IsReserved ? "занято" : "свободно")}]";
    }
    public class Customer
    {
        public string Name { get; }
        public List<Reservation> Reservations { get; } = new List<Reservation>();

        public Customer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя клиента не может быть пустым");
            Name = name;
        }

        public override string ToString() => Name;
    }
    public class Reservation
    {
        public Customer Customer { get; }
        public Movie Movie { get; }
        public Seat Seat { get; }
        public DateTime CreatedAt { get; }

        public Reservation(Customer customer, Movie movie, Seat seat)
        {
            Customer = customer;
            Movie = movie;
            Seat = seat;
            CreatedAt = DateTime.Now;
        }

        public override string ToString() =>
            $"{Customer.Name} → {Movie.Title}, место №{Seat.Number} ({CreatedAt:HH:mm:ss})";
    }
    public class CinemaHall
    {
        public string Name { get; }
        private readonly int _seatCount;
        private readonly Dictionary<Movie, List<Seat>> _seatsByMovie = new();

        public CinemaHall(string name, int seatCount, IEnumerable<Movie> movies)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название зала не может быть пустым");
            if (seatCount <= 0)
                throw new ArgumentException("Количество мест должно быть положительным");
            if (movies == null)
                throw new ArgumentNullException(nameof(movies));

            Name = name;
            _seatCount = seatCount;
            foreach (var movie in movies)
            {
                _seatsByMovie[movie] = Enumerable
                    .Range(1, seatCount)
                    .Select(n => new Seat(n, movie))
                    .ToList();
            }
        }
        public IReadOnlyList<Seat> GetSeats(Movie movie)
        {
            if (movie == null) throw new ArgumentNullException(nameof(movie));
            if (!_seatsByMovie.TryGetValue(movie, out var seats))
                throw new ArgumentException($"Для фильма {movie.Title} в зале нет мест");
            return seats;
        }

        public Seat GetSeat(Movie movie, int number)
        {
            var seat = GetSeats(movie).FirstOrDefault(s => s.Number == number);
            if (seat == null)
                throw new ArgumentException(
                    $"Места с номером {number} для фильма \"{movie.Title}\" не существует");
            return seat;
        }
        public void DisplayHall(Movie movie)
        {
            Console.WriteLine($"\n=== Схема зала \"{Name}\" на фильм {movie} ===");
            Console.WriteLine("Свободно: [ ]   Занято: [X]\n");

            var seats = GetSeats(movie);
            foreach (var seat in seats)
            {
                string marker = seat.IsReserved ? "[X]" : "[ ]";
                Console.Write($"{marker} {seat.Number,2}   ");
                if (seat.Number % 5 == 0) Console.WriteLine();
            }
            Console.WriteLine();

            int free = seats.Count(s => !s.IsReserved);
            int busy = seats.Count(s => s.IsReserved);
            Console.WriteLine($"Свободных мест: {free} | Занятых: {busy}");
        }

        public IEnumerable<Seat> GetFreeSeats(Movie movie) =>
            GetSeats(movie).Where(s => !s.IsReserved);

        public IEnumerable<Seat> GetReservedSeats(Movie movie) =>
            GetSeats(movie).Where(s => s.IsReserved);
    }
    public class BookingService
    {
        private readonly CinemaHall _hall;
        private readonly List<Movie> _movies;
        private readonly List<Reservation> _allReservations = new List<Reservation>();

        public BookingService(CinemaHall hall, IEnumerable<Movie> movies)
        {
            _hall = hall ?? throw new ArgumentNullException(nameof(hall));
            _movies = movies?.ToList() ?? throw new ArgumentNullException(nameof(movies));
        }

        public IReadOnlyList<Movie> Movies => _movies;
        public IReadOnlyList<Reservation> AllReservations => _allReservations;
        public CinemaHall Hall => _hall;

        public Movie SelectMovie(int index)
        {
            if (index < 0 || index >= _movies.Count)
                throw new ArgumentException("Неверный номер фильма");
            return _movies[index];
        }
        public Reservation MakeReservation(Customer customer, Movie movie, int seatNumber)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            if (movie == null) throw new ArgumentNullException(nameof(movie));

            var seat = _hall.GetSeat(movie, seatNumber);
            seat.Reserve();

            var reservation = new Reservation(customer, movie, seat);
            customer.Reservations.Add(reservation);
            _allReservations.Add(reservation);

            return reservation;
        }
        public void CancelReservation(Reservation reservation)
        {
            if (reservation == null) throw new ArgumentNullException(nameof(reservation));

            reservation.Seat.CancelReservation();
            reservation.Customer.Reservations.Remove(reservation);
            _allReservations.Remove(reservation);
        }
    }
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var movies = new List<Movie>
            {
                new Movie("Интерстеллар", 169),
                new Movie("Начало", 148),
                new Movie("Дюна", 155)
            };
            var hall = new CinemaHall("Зал №1", 10, movies);
            var service = new BookingService(hall, movies);
            Console.Write("Введите ваше имя: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = "Гость";
            var customer = new Customer(name);

            bool running = true;

            while (running)
            {
                Console.WriteLine("\n========== МЕНЮ ==========");
                Console.WriteLine("1. Показать список фильмов");
                Console.WriteLine("2. Забронировать место");
                Console.WriteLine("3. Показать схему зала");
                Console.WriteLine("4. Мои бронирования");
                Console.WriteLine("5. Отменить бронирование");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ShowMovies(service); break;
                        case "2": BookSeat(service, customer); break;
                        case "3": ShowHall(service); break;
                        case "4": ShowCustomerReservations(customer); break;
                        case "5": CancelReservation(service, customer); break;
                        case "0": running = false; break;
                        default: Console.WriteLine("Неверный выбор!"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ОШИБКА] {ex.Message}");
                }
            }

            Console.WriteLine("\nСпасибо за использование системы бронирования!");
        }

        static void ShowMovies(BookingService service)
        {
            Console.WriteLine("\n=== Список фильмов ===");
            for (int i = 0; i < service.Movies.Count; i++)
                Console.WriteLine($"{i + 1}. {service.Movies[i]}");
        }

        static void BookSeat(BookingService service, Customer customer)
        {
            ShowMovies(service);
            Console.Write("Выберите номер фильма: ");
            if (!int.TryParse(Console.ReadLine(), out int movieIndex))
                throw new ArgumentException("Некорректный номер фильма");

            var movie = service.SelectMovie(movieIndex - 1);
            service.Hall.DisplayHall(movie);

            Console.Write("Введите номер места для бронирования: ");
            if (!int.TryParse(Console.ReadLine(), out int seatNumber))
                throw new ArgumentException("Некорректный номер места");

            var reservation = service.MakeReservation(customer, movie, seatNumber);
            Console.WriteLine($"\n✓ УСПЕХ! {reservation}");
        }

        static void ShowHall(BookingService service)
        {
            ShowMovies(service);
            Console.Write("Выберите номер фильма для просмотра схемы зала: ");
            if (!int.TryParse(Console.ReadLine(), out int movieIndex))
                throw new ArgumentException("Некорректный номер фильма");

            var movie = service.SelectMovie(movieIndex - 1);
            service.Hall.DisplayHall(movie);
        }

        static void ShowCustomerReservations(Customer customer)
        {
            Console.WriteLine($"\n=== Бронирования клиента \"{customer.Name}\" ===");
            if (customer.Reservations.Count == 0)
            {
                Console.WriteLine("У вас нет активных бронирований.");
                return;
            }

            for (int i = 0; i < customer.Reservations.Count; i++)
                Console.WriteLine($"{i + 1}. {customer.Reservations[i]}");
        }

        static void CancelReservation(BookingService service, Customer customer)
        {
            ShowCustomerReservations(customer);
            if (customer.Reservations.Count == 0) return;

            Console.Write("Введите номер бронирования для отмены: ");
            if (!int.TryParse(Console.ReadLine(), out int index))
                throw new ArgumentException("Некорректный номер");

            if (index < 1 || index > customer.Reservations.Count)
                throw new ArgumentException("Бронирования с таким номером не существует");

            var reservation = customer.Reservations[index - 1];
            service.CancelReservation(reservation);
            Console.WriteLine($"\n✓ Бронирование отменено: {reservation}");
        }
    }
}