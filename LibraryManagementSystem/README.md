# Library Management System API

A RESTful Web API built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**, demonstrating solid **OOP principles** and clean architecture.

---

## Tech Stack

- .NET 8 / C#
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server (LocalDB)
- Swagger / OpenAPI

---

## OOP Concepts Applied

| Concept | Where Applied |
|---|---|
| **Inheritance** | `BaseEntity` — all entities inherit `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted` |
| **Encapsulation** | Business logic inside models (e.g., `Book.BorrowCopy()`, `Member.CanBorrow()`) |
| **Abstraction** | `IRepository<T>`, `IBookService`, `IMemberService`, etc. |
| **Polymorphism** | Generic `Repository<T>` works for any entity |

---

## Database Design

```
Library (1) ──── (N) Book
Member  (1) ──── (N) BorrowRecord
Book    (1) ──── (N) BorrowRecord
```

### Tables
- **Libraries** — Id, Name, Address, Phone, Email
- **Books** — Id, Title, Author, ISBN, Genre, PublishedYear, TotalCopies, AvailableCopies, LibraryId (FK)
- **Members** — Id, FullName, Email, Phone, Address, MembershipDate, IsActive
- **BorrowRecords** — Id, BookId (FK), MemberId (FK), BorrowDate, DueDate, ReturnDate, Status

---

## API Endpoints

### Libraries
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/libraries` | Get all libraries |
| GET | `/api/libraries/{id}` | Get library by ID |
| POST | `/api/libraries` | Create a new library |
| PUT | `/api/libraries/{id}` | Update a library |
| DELETE | `/api/libraries/{id}` | Delete a library |

### Books
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books` | Get all books |
| GET | `/api/books/{id}` | Get book by ID |
| GET | `/api/books/available` | Get all available books |
| GET | `/api/books/library/{libraryId}` | Get books by library |
| POST | `/api/books` | Add a new book |
| PUT | `/api/books/{id}` | Update a book |
| DELETE | `/api/books/{id}` | Delete a book |

### Members
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/members` | Get all members |
| GET | `/api/members/{id}` | Get member by ID |
| POST | `/api/members` | Register a new member |
| PUT | `/api/members/{id}` | Update member details |
| DELETE | `/api/members/{id}` | Delete a member |
| PATCH | `/api/members/{id}/activate` | Activate a member |
| PATCH | `/api/members/{id}/deactivate` | Deactivate a member |

### Borrow Records
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/borrowrecords` | Get all borrow records |
| GET | `/api/borrowrecords/{id}` | Get borrow record by ID |
| GET | `/api/borrowrecords/member/{memberId}` | Get borrow history by member |
| GET | `/api/borrowrecords/overdue` | Get all overdue records |
| POST | `/api/borrowrecords/borrow` | Borrow a book |
| PUT | `/api/borrowrecords/{id}/return` | Return a book |

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server / LocalDB

### Run the project

```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API
dotnet run
```

Swagger UI will be available at: `http://localhost:{port}`
