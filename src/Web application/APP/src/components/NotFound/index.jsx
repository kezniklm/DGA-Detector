/**
 * @file index.js
 *
 * @brief Component for displaying a 404 Not Found page in a React application.
 *
 * This file contains the implementation of the NotFound functional component, which is responsible for rendering a 404 Not Found page in a React application. It displays a simple message indicating that the requested page was not found.
 *
 * The main functionalities of this file include:
 * - Rendering a 404 Not Found page with a heading and a message.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React from "react";
import "./style.css";

/**
 * Functional component for rendering a 404 Not Found page.
 *
 * @returns {JSX.Element} JSX representation of the 404 Not Found page.
 */
export default function NotFound() {

    return (
        <section className="Content">
            <div className="Content-Body">
                <div className="notFound">
                    <h1>404</h1>
                    <h2 className="Heading">Page Not Found</h2>
                </div>
            </div>
        </section>
    );
}
