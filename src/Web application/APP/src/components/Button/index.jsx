/**
 * @file index.js
 *
 * @brief Defines a reusable button component for React applications.
 *
 * This file contains the implementation of the Button component, which renders a customizable button element in React applications. It allows for various customization options such as button text, styles, and event handling.
 *
 * The main functionalities of this file include:
 * - Defining a functional React component for rendering buttons.
 * - Providing props for customizing button appearance and behavior.
 * - Importing CSS styles to apply predefined button styles.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React from "react"
import "./style.css";

/**
 * @brief Button component.
 *
 * Renders a button with customizable styles and sizes.
 *
 * @param {Object} props - The props for the button component.
 * @param {boolean} [props.warning=false] - Whether the button should have a warning style.
 * @param {boolean} [props.trans=false] - Whether the button should have a transparent style.
 * @param {boolean} [props.small=false] - Whether the button should have a small size.
 * @param {React.ReactNode} props.children - The content to be displayed inside the button.
 * @returns {React.ReactNode} A button element with the specified properties.
 */
export default function Button({ children, warning=false, trans=false, small = false, ...props }) {
    return (
        <button {...props} className={`Button  ${trans && "trans"} ${warning && "warning"} ${small && "small"}`}>{
children}</button>
    );
}
