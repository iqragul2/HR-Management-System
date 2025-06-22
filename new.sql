-- Create the database
CREATE DATABASE ORP_db;
GO
USE ORP_db;
GO
-- 1. Roles Table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- 2. Users Table (Login Info)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Password NVARCHAR(150) NOT NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- 3. Departments Table
CREATE TABLE Departments (
    DepartmentId NVARCHAR(10) PRIMARY KEY, -- e.g. D001
    Name NVARCHAR(100) NOT NULL UNIQUE,
	ImageUrl NVARCHAR (255)
);

-- 4. Employees Table
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(100),
    UserId INT NOT NULL UNIQUE,
    DepartmentId NVARCHAR(10) NULL,
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(150),
    Address NVARCHAR(200),
    Designation NVARCHAR(100),
    ProfileCompleted BIT DEFAULT 0,
	Gender NVARCHAR(20),
    DateOfBirth DATE,
    ProfileImageUrl NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

-- 5. Recruiter Requests Table
CREATE TABLE RecruiterRequests (
    RequestId INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    ContactInfo NVARCHAR(150) NOT NULL,
    Reason NVARCHAR(MAX) NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    DepartmentId NVARCHAR(10),
    Status NVARCHAR(50) DEFAULT 'Pending',  -- Pending, Approved, Rejected
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

-- 6. Applicants Table
CREATE TABLE Applicants (
    ApplicantId NVARCHAR(20) PRIMARY KEY, -- e.g. A0001
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    Status NVARCHAR(50) DEFAULT 'Not In Process', -- Not In Process, In Process, Hired, Banned
    AppliedDate DATETIME DEFAULT GETDATE()
);

-- 7. Vacancies Table
CREATE TABLE Vacancies (
    VacancyId NVARCHAR(20) PRIMARY KEY, -- e.g. V0001
    Title NVARCHAR(150) NOT NULL,
    DepartmentId NVARCHAR(10) NOT NULL,
    JobDescription NVARCHAR(MAX),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    Openings INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Open', -- Open, Closed, Suspended
    ClosingDate DATETIME,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (CreatedBy) REFERENCES Employees(EmployeeId)
);

-- 8. ApplicantVacancy Table (Mapping)
CREATE TABLE ApplicantVacancy (
    Id INT PRIMARY KEY IDENTITY,
    ApplicantId NVARCHAR(20) NOT NULL,
    VacancyId NVARCHAR(20) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Interview Scheduled', -- Interview Scheduled, Selected, Rejected, Not Required
    AttachedDate DATETIME DEFAULT GETDATE(),
    IsInterviewScheduled BIT DEFAULT 0,
	InterviewScheduledDate DATETIME NULL,
    InterviewerId INT NULL, -- Must belong to same Department
    FOREIGN KEY (ApplicantId) REFERENCES Applicants(ApplicantId),
    FOREIGN KEY (VacancyId) REFERENCES Vacancies(VacancyId),
    FOREIGN KEY (InterviewerId) REFERENCES Employees(EmployeeId)
);

-- 9. Interviews Table
CREATE TABLE Interviews (
    InterviewId INT PRIMARY KEY IDENTITY,
    ApplicantVacancyId INT NOT NULL,
    ScheduledDate DATETIME NOT NULL,
    InterviewerId INT NOT NULL,
    Result NVARCHAR(50) DEFAULT 'Pending', -- Pending, Selected, Rejected
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ApplicantVacancyId) REFERENCES ApplicantVacancy(Id),
    FOREIGN KEY (InterviewerId) REFERENCES Employees(EmployeeId)
);


-- Insert Roles
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('User'), ('HR');

-- Insert Departments
INSERT INTO Departments (DepartmentId, Name)
VALUES ('D001', 'HRD'), ('D002', 'IT'), ('D003', 'Finance'), ('D004', 'Marketing');

-- Insert Sample Users
INSERT INTO Users (FirstName, LastName, Email, Password, RoleId)
VALUES 
('Admin', 'One', 'admin@orp.com', 'admin123', 1);



select * from employees
select * from users
select* from vacancies
select*from interviews
select*from applicants
select * from applicantvacancy
select*from RecruiterRequests
select*from Departments


ALTER TABLE Employees
ADD JoinedDate DATETIME NOT NULL DEFAULT GETDATE();

CREATE TABLE ContactUs (
    Id INT PRIMARY KEY IDENTITY,
    [From] VARCHAR(255),
    Email VARCHAR(255), 
    Subject VARCHAR(255),
    Message varchar(255)
);
select * from ContactUs

CREATE TABLE AboutUs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Position NVARCHAR(100),
    ImagePath NVARCHAR(200),
    Bio NVARCHAR(MAX),
    TwitterUrl NVARCHAR(200),
    FacebookUrl NVARCHAR(200),
    InstagramUrl NVARCHAR(200),
    LinkedInUrl NVARCHAR(200)
);

select * from AboutUs
select * from RecruiterRequests
select * from Users
select * from Roles
INSERT INTO AboutUs
VALUES 
(
    'Ariba Ali',
    'Team Lead',
    '~/assets/img/person/person-f-10.webp',
    'I am a Team Lead of the company',
    'https://twitter.com/Ariba',
    'https://facebook.com/Ariba',
    'https://instagram.com/Ariba',
    'https://linkedin.com/in/Ariba'
),(
    ' Mehwish Tasleem',
    'Tester',
    '~/assets/img/person/person-f-3.webp',
    'I am a tester of the company',
    'https://twitter.com/Mehwish',
    'https://facebook.com/Mehwish',
    'https://instagram.com/Mehwish',
    'https://linkedin.com/in/Mehwish'
),
(
    ' Hadiqa',
    'Frontend Developer',
    '~/assets/img/person/person-f-7.webp',
    'I am a Frontend Developer of the company',
    'https://twitter.com/Hadiqa',
    'https://facebook.com/Hadiqa',
    'https://instagram.com/Hadiqa',
    'https://linkedin.com/in/Hadiqa'
),
(
    ' Iqra Gul',
    'Backend Developer',
    '~/assets/img/person/person-f-6.webp',
    'I am a Backend Developer of the company',
    'https://twitter.com/Iqra',
    'https://facebook.com/Iqra',
    'https://instagram.com/Iqra',
    'https://linkedin.com/in/Iqra'
),
(
    'Urooj Mehrab',
    'System Administrator',
    '~/assets/img/person/person-f-5.webp',
    'I am a System Administrator of the company',
    'https://twitter.com/Urooj',
    'https://facebook.com/Urooj',
    'https://instagram.com/Urooj',
    'https://linkedin.com/in/Urooj'
);



SELECT * FROM Employees WHERE UserId = 2;
