# RE-Waste

**A smart and easy way to find out what to do with your unwanted phone.**
Complete a short questionnaire and the wizard will suggest the best option for you.

## Features

RE-Waste is built around a wizard-style quiz. Users start it from the button on the home page or from the link in the navigation bar.

**For all visitors**
- Take the quiz: answer a few simple questions about your phone.
- See the most suitable options for the phone, with the reason behind each recommendation.
- See a list of drop-off points, shown on a map with the distance from the postcode you entered.
- Read our mission on the **About us** page.
- Contact the team using the form on the **Contact us** page.

**For registered users**
- Save quiz results with the save button.
- If you are logged in, you are taken to your own saved results page.
- If you are not, you are sent to the login page, where you can switch to registration if you do not have an account yet.

**For managers**
- An authorised manager can create, update and remove:
  - drop-off points
  - recommendation rules
  - wizard questions

## Tech Stack

- ASP.NET Core 8 MVC
- Entity Framework Core with SQL Server LocalDB
- ASP.NET Core Identity for user authentication and password hashing
- Unit tests in `RE-Waste.Tests`

## Getting Started

You will need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and SQL Server (or SQL Server LocalDB).

### Visual Studio
1. Choose **Git → Clone Repository** and paste `https://github.com/mawadub/ReWaste.git`.
2. Open `RE-Waste/RE-Waste.sln`.
3. Check the connection string in `RE-Waste/appsettings.json`. The app uses SQL Server LocalDB by default (installed with Visual Studio on Windows). If you use a different SQL Server, update DefaultConnection in RE-Waste/appsettings.json.
4. Press the green arrow to run the app.

### Visual Studio Code
1. Clone the repository:
   ```bash
   git clone https://github.com/mawadub/ReWaste.git
   ```
2. Install the **C# Dev Kit** extension; 
3. Check the connection string in `RE-Waste/appsettings.json`. Windows with LocalDB works out of the box; on Mac or Linux you have two options: Run SQL Server in Docker or use a shared or cloud SQL Server.
4. Run the app:
   ```bash
   cd ReWaste/RE-Waste
   dotnet run
   ```
5. When the build finishes, open the `localhost` address shown in the terminal.

### Manager account
Log in with `manager@rewaste.local` and the password `Manager123!`. These are in `RE-Waste/appsettings.Development.json`. Change them before any real deployment.


## First run
- The first run takes a little longer. The app creates the database and fills it with the starter questions, rules and drop-off points automatically, so there is nothing to set up by hand.
- If you have run the project before, delete the old `aspnet-RE_Waste-…` database first (in Visual Studio: **View → SQL Server Object Explorer**, then right-click the database and choose **Delete**). Otherwise the app reuses the old data.
- The new database may not appear in SQL Server Object Explorer until you right-click *Databases* and choose *Refresh*.
- If Visual Studio asks you to trust the HTTPS certificate, click *Yes*.
- The postcode lookup and the drop-off places need an internet connection.

## Project Structure

```
ReWaste/
├── RE-Waste/            Main web application
│   ├── Areas/           Identity and Manager controllers and views
│   ├── Controllers/     Public-facing controllers
│   ├── Data/            Database context and everything database-related
│   ├── Extensions/      Helper extensions
│   ├── Models/          Data models
│   ├── Services/        Helper classes and interfaces
│   ├── ViewModels/      Models used by the views
│   ├── Views/           Razor views
│   └── Program.cs       Application entry point
└── RE-Waste.Tests/      Unit tests
```

## Tests
### In Visual Studio:
1. Open RE-Waste/RE-Waste.sln. The RE-Waste.Tests project is part of the solution, so you’ll see it in Solution Explorer.
2. Open the Test Explorer from the top menu with Test → Test Explorer. The shortcut is Ctrl+E, T.
3. If the list is empty, build the solution first with Ctrl+Shift+B. Visual Studio finds the tests after a build.
4. Click the Run All Tests button (the double green arrow) at the top of the Test Explorer window.
5. To run just one test, right-click it in the list and choose Run. You can also right-click a test method in REWasteTests.cs and choose Run Tests.

### In Visual Studio Code:
1. Run the unit tests from the terminal:
  ```bash
  dotnet test
  ```

## Contributing
- Each branch covers one feature.
- Every pull request is reviewed by at least one other team member before it is merged.

## Licence and Contact

RE-Waste was created as a group project for a module at Glasgow Caledonian University by a team of eight students from the Software Development and AI and Data Science courses.

For questions, please open an issue in this repository.
