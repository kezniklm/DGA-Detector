# DGA-Detector Web Application

This web application serves as a tool for displaying results from the DGA Detector, facilitating the management of Blacklist and Whitelist entries.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [License](#license)

## Installation

To run this application locally, follow these steps:

1. Ensure you have Node.js and npm installed on your machine. You can download them from [here](https://nodejs.org/).

2. Clone this repository to your local machine using the following command:

   ```bash
   git clone https://github.com/kezniklm/DGA-Detector.git
   ```

3. Navigate to the project directory:

   ```bash
   cd DGA-Detector/src/Web\ application/APP
   ```

4. Install dependencies:

   ```bash
   npm install
   ```

## Usage

Once the installation is complete, start the development server:

```bash
npm run-script dev
```

This command will launch the application locally. Open your web browser and go to [http://localhost:5173](http://localhost:5173) to view it.

## Technologies Used

- React.js
- npm
- Vite
- ESLint

## Project Structure

```
DGA-Detector
└── src
    └── Web application
        └── APP
            ├── .eslintrc.cjs
            ├── .gitignore
            ├── app.esproj
            ├── config.js
            ├── index.html
            ├── nuget.config
            ├── package-lock.json
            ├── package.json
            ├── README.md
            ├── vite.config.js
            └── src
                ├── assets
                ├── components
                │   ├── Add
                │   ├── Blacklist
                │   ├── Button
                │   ├── ChangePassword
                │   ├── ConfirmPopup
                │   ├── DetectionResults
                │   ├── ForgotPassword
                │   ├── HomePage
                │   ├── Login
                │   ├── Navbar
                │   ├── NotFound
                │   ├── Profile
                │   ├── Register
                │   ├── Table
                │   ├── Update
                │   └── WhiteList
                └── utils
```

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../../../LICENSE) file for details.
