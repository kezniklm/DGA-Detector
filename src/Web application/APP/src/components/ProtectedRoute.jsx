/**
 * @file ProtectedRoute.js
 *
 * @brief Provides a protected routing mechanism in a React application.
 *
 * This file contains the implementation of the ProtectedRoute component which utilizes React Router DOM to manage route protection based on authentication status.
 * It checks if the user is authenticated using the `useAuth` hook. If authenticated, the specified component is rendered, otherwise, the user is redirected to the login page.
 *
 * The main functionalities of this file include:
 * - Checking user authentication status.
 * - Rendering the passed React component upon successful authentication.
 * - Redirecting to the login page if the user is not authenticated.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React from "react";
import { Route, Navigate } from "react-router-dom";
import useAuth from "../utils/useAuth";

/**
 * @function ProtectedRoute
 * @brief Renders a protected route component.
 *
 * This component checks if the user is authenticated using the `useAuth` hook. If the user is authenticated,
 * it renders the specified component. If not, it redirects the user to the login page.
 *
 * @param {Object} props - The properties passed to the ProtectedRoute component, which include:
 * @param {React.Component} component - The React component to render if authenticated.
 * @param {Object} rest - Additional properties to be passed to the underlying Route component.
 *
 * @returns {Route} A Route component that either renders the passed component or redirects to the login page.
 */
export default function ProtectedRoute({ component: Component, ...rest }) {
    const isLoggedIn = useAuth();

    return (
        <Route
            {...rest}
            render={(props) =>
        isLoggedIn ? <Component {...props}/> : <Navigate to="/login" replace/>
      }/>
    );
}
