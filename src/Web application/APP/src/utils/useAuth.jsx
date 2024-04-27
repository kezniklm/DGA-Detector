/**
 * @file useAuth.js
 *
 * @brief Provides authentication state management using React hooks.
 *
 * This file contains the implementation of the useAuth custom hook, which manages the login status of a user within a React application. It utilizes React's useState and useEffect hooks for managing state and side effects.
 *
 * The main functionalities of this file include:
 * - Initializing and managing the `isLoggedIn` state.
 * - Checking sessionStorage for any previously stored login status upon component mounting.
 * - Updating the login status based on the sessionStorage value.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

import { useState, useEffect } from "react";

/**
 * useAuth custom hook.
 *
 * @details
 * This hook initializes the `isLoggedIn` state to false and checks sessionStorage
 * for any previously stored login status upon component mounting. The login status
 * is then updated based on the value retrieved from sessionStorage.
 *
 * @return {boolean} The current login status, `true` if the user is logged in and
 * `false` otherwise.
 */
export default function useAuth() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    useEffect(() => {
            const storedLoginStatus = sessionStorage.getItem("isLoggedIn");
            setIsLoggedIn(JSON.parse(storedLoginStatus));
        },
        []);

    return isLoggedIn;
}
