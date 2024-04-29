/**
 * @file main.js
 *
 * @brief Entry point for the React application.
 *
 * This file is responsible for initializing and rendering the root React component into the DOM. It utilizes ReactDOM for managing the rendering lifecycle and React component tree. The main functionality of this file is to mount the top-level component, `App`, onto the root DOM element.
 *
 * Features included in this file:
 * - Importing React library to support JSX syntax and React component logic.
 * - Importing ReactDOM to enable efficient DOM manipulations and updates.
 * - Importing the main App component which acts as the root of the React component hierarchy.
 * - Importing CSS for global styling.
 * - Rendering the App component into the DOM at the designated 'root' node.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React from "react"
import ReactDOM from "react-dom/client"
import App from "./App.jsx"
import "./index.css"

ReactDOM.createRoot(document.getElementById("root")).render(
    <App/>
);
