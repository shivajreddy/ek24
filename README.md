
# EK24 - Custom Kitchen Design, Build, Price, and Estimate Tool

## Table of Contents
- [Project Overview](#project-overview)
- [Features](#features)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)
- [Contact Information](#contact-information)

## Project Overview

EK24 is a comprehensive tool designed for Eagle of VA, focusing on the custom design, pricing, and estimation of kitchen projects. This project automates various tasks associated with managing kitchen design projects, from planning and resource allocation to cost estimation and final proposal generation.

The primary objective of EK24 is to streamline the workflow of kitchen project management, ensuring accuracy, efficiency, and ease of use. The tool provides a user-friendly interface, robust back-end logic, and customizable options to meet specific project requirements.

## Features

- **Custom Kitchen Design**: Allows users to design kitchens tailored to client specifications, integrating with various design libraries and templates.
- **Price Estimation**: Calculates costs based on the materials, labor, and other factors involved in the project, ensuring accurate budgeting.
- **Project Management**: Provides a streamlined process for managing kitchen projects, from initial design to final delivery.
- **Request Handling**: Manages client requests and integrates them into the project workflow efficiently.
- **Resource Management**: Handles the allocation of resources such as materials, labor, and time, optimizing project execution.
- **Command Integration**: Offers a wide range of commands to interact with the Revit API, enabling advanced customization and control over the design and build process.

## Project Structure

The EK24 project is organized into several key folders and components:

- **App**: Contains the main application logic and entry points.
- **Commands**: Houses all command classes that communicate with the Revit API, performing operations such as design modifications, material selections, and more.
- **Models**: Defines the data models used across the project, including project entities like Kitchen, Material, and Estimate.
- **ViewModels**: Includes ViewModel classes following the MVVM pattern, handling data binding between the UI and the underlying data models.
- **Views**: Contains the XAML files and UI components that make up the user interface of the tool.
- **Utils**: Utility classes that provide helper functions and methods used across the project.
- **RequestHandlingUtils**: Specialized utilities for managing client requests and integrating them into the project workflow.
- **Resources**: Contains static resources such as images, icons, and other media used within the application.

## Installation

To set up the EK24 project on your local machine, follow these steps:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shivajreddy/ek24.git
   ```

2. **Navigate to the project directory**:
   ```bash
   cd ek24
   ```

3. **Install dependencies**:
   Ensure that you have the necessary dependencies installed by running the following command (if applicable):
   ```bash
   dotnet restore
   ```

4. **Build the project**:
   Compile the project using:
   ```bash
   dotnet build
   ```

5. **Run the project**:
   Launch the application using:
   ```bash
   dotnet run
   ```

## Usage

Once installed and running, EK24 provides an intuitive interface for designing and managing kitchen projects. Users can create new projects, define materials and costs, and generate estimates based on predefined templates or custom designs.

### Key Commands
- **CreateProject**: Initializes a new kitchen project.
- **EstimateCost**: Generates a detailed cost estimate for the project based on input parameters.
- **DesignKitchen**: Launches the design interface, allowing users to build custom kitchen layouts.

### Common Workflows
1. **Starting a New Project**: Use the `CreateProject` command to set up a new kitchen design project.
2. **Designing a Kitchen**: Navigate to the design interface using `DesignKitchen` and begin customizing the layout.
3. **Generating an Estimate**: After finalizing the design, use `EstimateCost` to produce a detailed cost breakdown.

## Configuration

EK24 offers a range of configuration options to tailor the tool to specific project requirements. These options can be adjusted in the `config.json` file, located in the project root.

### Example Configuration
```json
{
    "defaultMaterialCost": 50,
    "defaultLaborRate": 75,
    "currency": "USD",
    "taxRate": 0.07
}
```

## Contributing

Contributions to EK24 are welcome! If you find a bug or have a feature request, please open an issue on GitHub. You can also fork the repository, make your changes, and submit a pull request.

### Contribution Guidelines
1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -am 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Open a pull request.

## License

EK24 is licensed under the MIT License. See the [LICENSE](https://github.com/shivajreddy/ek24/blob/main/LICENSE) file for more details.

## Contact Information

For any questions or inquiries, feel free to contact the project maintainer:

- **Name**: Shiva Reddy
- **Email**: shivajreddy@outlook.com
- **GitHub**: [shivajreddy](https://github.com/shivajreddy)
- **LinkedIn**: [Shiva Reddy](https://www.linkedin.com/in/kshivareddy/)

---

This README file provides a detailed overview of the EK24 project, covering its purpose, features, structure, and usage. By following the instructions outlined above, users and developers can effectively utilize and contribute to the project.
