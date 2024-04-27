/**
 * @file App.js
 *
 * @brief Main application component file for managing user interfaces and navigation.
 *
 * This file contains the main application component 'App' which sets up and manages user authentication states,
 * navigation routes, and overall component rendering within the application using React Router.
 * Utilizes hooks for state management and effects, and integrates various child components such as Navbar, HomePage,
 * and Login among others. Additionally, it handles user session management and toast notifications for a better user experience.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import { useEffect, useState } from "react";
import "./App.css";
import Navbar from "./components/Navbar";
import HomePage from "./components/HomePage";
import DetectionResults from "./components/DetectionResults";
import Blacklist from "./components/Blacklist";
import WhiteList from "./components/WhiteList";
import Profile from "./components/Profile";
import ChangePassword from "./components/ChangePassword";
import Login from "./components/Login";
import Register from "./components/Register";
import NotFound from "./components/NotFound";
import ForgotPassword from "./components/ForgotPassword";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import API_URL from "../config";

/**
 * @function App
 * @brief Main component function that sets up the router and state management for user authentication.
 *
 * Uses useState to manage authentication state and useEffect to check sessionStorage for existing login state.
 * Renders the application routes and components based on the current authentication state.
 * @returns React Element that includes the Router and associated routes.
 */
function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isUserUpdate, setIsUserUpdate] = useState(null);

    /**
     * @function handleLogin
     * @brief Handles user login by setting session storage and updating isLoggedIn state.
     * @param userData Object containing user data, typically includes username and token.
     */
    const handleLogin = (userData) => {
        sessionStorage.setItem("isLoggedIn", JSON.stringify(true));
        setIsLoggedIn(true);
    };

    /**
     * @function handleLogout
     * @brief Logs the user out by removing authentication data from session storage and updating state.
     */
    const handleLogout = () => {
        setIsLoggedIn(false);
        toast.info("Logged Out!");
        sessionStorage.removeItem("isLoggedIn");
    };

    /**
     * @hook useEffect
     * @brief React useEffect hook to check for existing login state in sessionStorage on component mount.
     */
    useEffect(() => {
            const storedLoginStatus = sessionStorage.getItem("isLoggedIn");
            if (storedLoginStatus) {
                setIsLoggedIn(JSON.parse(storedLoginStatus));
            }
        },
        []);

    return (
        <Router>
            <main>
                <ToastContainer
                    position="top-center"
                    autoClose={2000}
                    hideProgressBar={false}
                    newestOnTop={false}
                    closeOnClick
                    rtl={false}
                    pauseOnFocusLoss
                    draggable
                    pauseOnHover
                    theme="dark"/>
                <Navbar
                    handleLogout={handleLogout}
                    isLoggedIn={isLoggedIn}
                    isUserUpdate={isUserUpdate}/>

                <Routes>
                    <Route path="/" element={<HomePage isLoggedIn={isLoggedIn}/>}/>
                    <Route
                        path="/DetectionResults"
                        element={<DetectionResults isLoggedIn={isLoggedIn}/>}/>
                    <Route
                        path="/Blacklist"
                        element={<Blacklist isLoggedIn={isLoggedIn}/>}/>
                    <Route
                        path="/Whitelist"
                        element={<WhiteList isLoggedIn={isLoggedIn}/>}/>
                    <Route
                        path="/Profile"
                        element={
              <Profile
                  isLoggedIn={isLoggedIn}
                  isUserUpdate={isUserUpdate}
                  setIsUserUpdate={setIsUserUpdate}/>
            }/>
                    <Route
                        path="/ChangePassword"
                        element={<ChangePassword isLoggedIn={isLoggedIn}/>}/>
                    <Route path="/Login" element={<Login handleLogin={handleLogin}/>}/>
                    <Route path="/ForgotPassword" element={<ForgotPassword/>}/>
                    <Route
                        path="/Register"
                        element={<Register handleLogin={handleLogin}/>}/>

                    <Route path="*" element={<NotFound/>}/>
                </Routes>
            </main>
        </Router>
    );
}

export default App;
