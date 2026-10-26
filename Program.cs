using LibrarySystem.Data;
using LibrarySystem.Entities;

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Entities;

namespace LibrarySystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new LibraryDbContextFactory();
            using var context = factory.CreateDbContext(args);

            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("============================================");
                Console.WriteLine("        Library Management System           ");
                Console.WriteLine("============================================");
                Console.WriteLine("1. Add Category");
                Console.WriteLine("2. Add Author");
                Console.WriteLine("3. Add Book (with Inventory & Category)");
                Console.WriteLine("4. Add Borrower");
                Console.WriteLine("5. Borrow a Book");
                Console.WriteLine("6. Return a Book (with Fine Calculation)");
                Console.WriteLine("7. List Books & Inventory");
                Console.WriteLine("8. View Active Loans Report");
                Console.WriteLine("9. Search for a Book");
                Console.WriteLine("10. List All Borrowers");
                Console.WriteLine("11. View Complete Loan History Report");
                Console.WriteLine("--------------------------------------------");
                Console.WriteLine("12. Exit");
                Console.WriteLine("============================================");
                Console.Write("Select an option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddCategory(context);
                        break;
                    case "2":
                        AddAuthor(context);
                        break;
                    case "3":
                        AddBook(context);
                        break;
                    case "4":
                        AddBorrower(context);
                        break;
                    case "5":
                        BorrowBook(context);
                        break;
                    case "6":
                        ReturnBook(context);
                        break;
                    case "7":
                        ListBooks(context);
                        break;
                    case "8":
                        ViewActiveLoansReport(context);
                        break;
                    case "9":
                        SearchBooks(context);
                        break;
                    case "10":
                        ListBorrowers(context);
                        break;
                    case "11":
                        ViewLoanHistoryReport(context);
                        break;
                    case "12":
                        exit = true;
                        Console.WriteLine("\nThank you for using the Library Management System!");
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        #region Operations

        static void AddCategory(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Add New Category ---");
                string name = SafeReadString("Enter Category Name: ");

                var category = new Category { Name = name };
                context.Categories.Add(category);
                context.SaveChanges();

                Console.WriteLine("\nCategory added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving: " + ex.Message);
            }
            Wait();
        }

        static void AddAuthor(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Add New Author ---");
                string name = SafeReadString("Enter Author Name: ");

                var author = new Author { Name = name };
                context.Authors.Add(author);
                context.SaveChanges();

                Console.WriteLine("\nAuthor added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving: " + ex.Message);
            }
            Wait();
        }

        static void AddBook(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Add New Book ---");

                var categories = context.Categories.ToList();
                var authors = context.Authors.ToList();

                if (!categories.Any() || !authors.Any())
                {
                    Console.WriteLine("\nPlease add at least one Category and one Author before adding books.");
                    Wait();
                    return;
                }

                string title = SafeReadString("Enter Book Title: ");
                string isbn = SafeReadString("Enter ISBN: ");

                Console.WriteLine("\nAvailable Categories:");
                foreach (var c in categories) Console.WriteLine($"[ID: {c.Id}] - {c.Name}");
                int categoryId = SafeReadInt("Select Category ID: ");

                Console.WriteLine("\nAvailable Authors:");
                foreach (var a in authors) Console.WriteLine($"[ID: {a.Id}] - {a.Name}");
                int authorId = SafeReadInt("Select Author ID: ");

                int totalCopies = SafeReadInt("Enter Total Copies: ");
                if (totalCopies <= 0) totalCopies = 1;

                var book = new Book
                {
                    Title = title,
                    ISBN = isbn,
                    CategoryId = categoryId,
                    AuthorId = authorId,
                    TotalCopies = totalCopies,
                    AvailableCopies = totalCopies // Initially all copies are available
                };

                context.Books.Add(book);
                context.SaveChanges();

                Console.WriteLine("\nBook and inventory added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving: " + ex.Message);
            }
            Wait();
        }

        static void AddBorrower(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Add New Borrower ---");

                string name = SafeReadString("Enter Borrower Name: ");
                Console.Write("Enter Email Address: ");
                string email = Console.ReadLine();

                var borrower = new Borrower
                {
                    Name = name,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    MembershipDate = DateTime.Now
                };

                context.Borrowers.Add(borrower);
                context.SaveChanges();
                Console.WriteLine("\nBorrower registered successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving: " + ex.Message);
            }
            Wait();
        }

        static void BorrowBook(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Borrow a Book ---");

                var books = context.Books.Where(b => b.AvailableCopies > 0).ToList();
                var borrowers = context.Borrowers.ToList();

                if (!books.Any() || !borrowers.Any())
                {
                    Console.WriteLine("\nNo available books with copies or no borrowers found.");
                    Wait();
                    return;
                }

                Console.WriteLine("Available Books (with stock):");
                foreach (var b in books)
                    Console.WriteLine($"[ID: {b.Id}] - {b.Title} (Available: {b.AvailableCopies}/{b.TotalCopies})");

                int bookId = SafeReadInt("Select Book ID: ");

                Console.WriteLine("\nRegistered Borrowers:");
                foreach (var br in borrowers) Console.WriteLine($"[ID: {br.Id}] - {br.Name}");
                int borrowerId = SafeReadInt("Select Borrower ID: ");

                var selectedBook = context.Books.FirstOrDefault(b => b.Id == bookId);
                var selectedBorrower = context.Borrowers.FirstOrDefault(br => br.Id == borrowerId);

                if (selectedBook != null && selectedBorrower != null && selectedBook.AvailableCopies > 0)
                {
                    // Decrement available copies
                    selectedBook.AvailableCopies--;

                    var loan = new Loan
                    {
                        BookId = bookId,
                        BorrowerId = borrowerId,
                        LoanDate = DateTime.Now,
                        FineAmount = 0.00m
                    };

                    context.Loans.Add(loan);
                    context.SaveChanges();
                    Console.WriteLine("\nLoan recorded successfully and inventory updated!");
                }
                else
                {
                    Console.WriteLine("\nInvalid selection or book is out of stock.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            Wait();
        }

        static void ReturnBook(LibraryDbContext context)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("--- Return a Book ---");

                var activeLoans = context.Loans
                    .Include(l => l.Book)
                    .Include(l => l.Borrower)
                    .Where(l => l.ReturnDate == null)
                    .ToList();

                if (!activeLoans.Any())
                {
                    Console.WriteLine("\nThere are no active loans to return.");
                    Wait();
                    return;
                }

                Console.WriteLine("Active Loans:");
                foreach (var l in activeLoans)
                {
                    Console.WriteLine($"Book ID: {l.BookId} ({l.Book.Title}) | Borrower ID: {l.BorrowerId} ({l.Borrower.Name}) | Loan Date: {l.LoanDate:d}");
                }

                int bookId = SafeReadInt("\nEnter Book ID to return: ");
                int borrowerId = SafeReadInt("Enter Borrower ID: ");

                var loanToReturn = activeLoans.FirstOrDefault(l => l.BookId == bookId && l.BorrowerId == borrowerId);

                if (loanToReturn != null)
                {
                    loanToReturn.ReturnDate = DateTime.Now;

                    // Calculate fine: Allowed 14 days, 5.00 EGP per extra day overdue
                    int allowedDays = 14;
                    int daysElapsed = (DateTime.Now - loanToReturn.LoanDate).Days;
                    decimal fineRatePerDay = 5.00m;

                    if (daysElapsed > allowedDays)
                    {
                        int overdueDays = daysElapsed - allowedDays;
                        loanToReturn.FineAmount = overdueDays * fineRatePerDay;
                    }
                    else
                    {
                        loanToReturn.FineAmount = 0.00m;
                    }

                    // Increment available copies back
                    if (loanToReturn.Book != null)
                    {
                        loanToReturn.Book.AvailableCopies++;
                    }

                    context.SaveChanges();
                    Console.WriteLine($"\nBook returned successfully!");
                    Console.WriteLine($"Return Date: {loanToReturn.ReturnDate.Value:d}");
                    Console.WriteLine($"Calculated Fine Amount: {loanToReturn.FineAmount:C}");
                }
                else
                {
                    Console.WriteLine("\nActive loan record not found for the given IDs.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            Wait();
        }

        static int SafeReadInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out result))
                {
                    return result;
                }
                Console.WriteLine("Error: Please enter a valid integer.");
            }
        }

        static string SafeReadString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }
                Console.WriteLine("Error: This field cannot be empty.");
            }
        }

        static void ListBooks(LibraryDbContext context)
        {
            Console.Clear();
            Console.WriteLine("--- Book Catalog & Inventory ---");

            var books = context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToList();

            if (!books.Any())
            {
                Console.WriteLine("\nNo books found in the database.");
            }
            else
            {
                Console.WriteLine("{0,-5} | {1,-25} | {2,-15} | {3,-15} | {4,-10}", "ID", "Book Title", "Category", "Author", "Stock (Avail/Total)");
                Console.WriteLine(new string('-', 85));
                foreach (var b in books)
                {
                    string stockStr = $"{b.AvailableCopies}/{b.TotalCopies}";
                    Console.WriteLine("{0,-5} | {1,-25} | {2,-15} | {3,-15} | {4,-10}",
                        b.Id, b.Title, b.Category?.Name ?? "N/A", b.Author?.Name ?? "N/A", stockStr);
                }
            }
            Wait();
        }

        static void ListBorrowers(LibraryDbContext context)
        {
            Console.Clear();
            Console.WriteLine("--- Registered Borrowers ---");

            var borrowers = context.Borrowers.ToList();

            if (!borrowers.Any())
            {
                Console.WriteLine("\nNo borrowers found in the database.");
            }
            else
            {
                Console.WriteLine("{0,-5} | {1,-25} | {2,-25} | {3,-15}", "ID", "Name", "Email", "Membership Date");
                Console.WriteLine(new string('-', 78));
                foreach (var b in borrowers)
                {
                    Console.WriteLine("{0,-5} | {1,-25} | {2,-25} | {3,-15:d}", b.Id, b.Name, b.Email ?? "N/A", b.MembershipDate);
                }
            }
            Wait();
        }

        static void ViewActiveLoansReport(LibraryDbContext context)
        {
            Console.Clear();
            Console.WriteLine("--- Active Loans Report ---");

            var activeLoans = context.Loans
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .Where(l => l.ReturnDate == null)
                .ToList();

            if (!activeLoans.Any())
            {
                Console.WriteLine("\nNo active loans found in the system.");
            }
            else
            {
                Console.WriteLine("{0,-8} | {1,-10} | {2,-22} | {3,-15} | {4,-12} | {5,-10}", "Book ID", "Borrower ID", "Book Title", "Borrower Name", "Loan Date", "Fine Acc.");
                Console.WriteLine(new string('-', 90));
                foreach (var l in activeLoans)
                {
                    // Estimate current fine if overdue
                    int daysElapsed = (DateTime.Now - l.LoanDate).Days;
                    decimal currentFine = daysElapsed > 14 ? (daysElapsed - 14) * 5.00m : 0.00m;

                    Console.WriteLine("{0,-8} | {1,-10} | {2,-22} | {3,-15} | {4,-12:d} | {5,-10:C}",
                        l.BookId, l.BorrowerId, l.Book.Title, l.Borrower.Name, l.LoanDate, currentFine);
                }
            }
            Wait();
        }

        static void ViewLoanHistoryReport(LibraryDbContext context)
        {
            Console.Clear();
            Console.WriteLine("--- Complete Loan History Report ---");

            var allLoans = context.Loans
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .ToList();

            if (!allLoans.Any())
            {
                Console.WriteLine("\nNo loan records found in the system.");
            }
            else
            {
                Console.WriteLine("{0,-8} | {1,-10} | {2,-20} | {3,-15} | {4,-10} | {5,-12} | {6,-10}",
                    "Book ID", "Borrower ID", "Book Title", "Borrower Name", "Loan Date", "Status", "Fine");
                Console.WriteLine(new string('-', 95));
                foreach (var l in allLoans)
                {
                    string status = l.ReturnDate.HasValue ? $"Returned" : "Active";
                    Console.WriteLine("{0,-8} | {1,-10} | {2,-20} | {3,-15} | {4,-10:d} | {5,-12} | {6,-10:C}",
                        l.BookId, l.BorrowerId, l.Book.Title, l.Borrower.Name, l.LoanDate, status, l.FineAmount);
                }
            }
            Wait();
        }

        static void SearchBooks(LibraryDbContext context)
        {
            Console.Clear();
            Console.WriteLine("--- Search Books ---");
            Console.Write("Enter search keyword (Title or Author): ");
            string keyword = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                Console.WriteLine("\nSearch keyword cannot be empty.");
                Wait();
                return;
            }

            var results = context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.ToLower().Contains(keyword) || b.Author.Name.ToLower().Contains(keyword))
                .ToList();

            if (!results.Any())
            {
                Console.WriteLine("\nNo matching books found.");
            }
            else
            {
                Console.WriteLine("\nSearch Results:");
                Console.WriteLine("{0,-5} | {1,-25} | {2,-15} | {3,-15} | {4,-10}", "ID", "Book Title", "Category", "Author", "Stock");
                Console.WriteLine(new string('-', 80));
                foreach (var b in results)
                {
                    string stockStr = $"{b.AvailableCopies}/{b.TotalCopies}";
                    Console.WriteLine("{0,-5} | {1,-25} | {2,-15} | {3,-15} | {4,-10}",
                        b.Id, b.Title, b.Category?.Name ?? "N/A", b.Author?.Name ?? "N/A", stockStr);
                }
            }
            Wait();
        }

        static void Wait()
        {
            Console.WriteLine("\nPress Enter to return to the main menu...");
            Console.ReadLine();
        }

        #endregion
    }
}