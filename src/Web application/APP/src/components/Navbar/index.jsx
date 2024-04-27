/**
 * @file index.js
 *
 * @brief Provides navigation bar functionality for the application.
 *
 * This file contains the implementation of the Navbar component, which serves as the navigation bar for the application. It displays links to different sections of the application and handles user authentication-related functionalities.
 *
 * The main functionalities of this file include:
 * - Displaying navigation links based on user authentication status.
 * - Fetching user data for authenticated users.
 * - Animating the navigation bar using GSAP library.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useEffect, useState } from "react";
import "./style.css";
import avatar from "../../assets/avatar.jpg";
import { Link, useLocation } from "react-router-dom";
import API_URL from "../../../config";
import useAuth from "../../utils/useAuth";
import gsap from "gsap";
import { toast } from "react-toastify";

/**
 * @brief Navbar component
 * @param {Object} props - Component props.
 * @param {Function} props.handleLogout - Function to handle user logout.
 * @param {boolean} props.isLoggedIn - Boolean indicating whether the user is logged in.
 * @param {boolean} props.isUserUpdate - Boolean indicating whether user data has been updated.
 * @returns {JSX.Element} JSX element representing the Navbar component.
 */
export default function Navbar({ handleLogout, isLoggedIn, isUserUpdate }) {
    const location = useLocation();
    const [user, setUser] = useState({});

    /**
     * @brief Checks if a given pathname is the active location.
     * @param {string} pathname - The pathname to compare.
     * @returns {boolean} True if the pathname is active, otherwise false.
     */
    const isLinkActive = (pathname) => {
        return location.pathname === pathname;
    };

    const data = [
        {
            label: "Home",
            url: "/",
            img: null,
        },
        {
            label: "Detection Results",
            url: "/DetectionResults",
            img: null,
        },
        {
            label: "Blacklist",
            url: "/Blacklist",
            img: null,
        },
        {
            label: "Whitelist",
            url: "/Whitelist",
            img: null,
        },
        {
            label: user?.displayUserName || "USER NAME",
            url: "/profile",
            img: user?.photoUrl || avatar,
            dropdown: [
                {
                    label: "Profile",
                    url: "/profile",
                },
                {
                    label: "Logout",
                    url: "/",
                    onclick: { handleLogout },
                },
            ],
        },
    ];

    useEffect(() => {
            gsap.fromTo(".Navbar",
                {
                    opacity: 0,
                    y: -100,
                },
                {
                    y: 0,
                    delay: 1,
                    duration: 1,
                    opacity: 1,
                }
            );
        },
        []);

    useEffect(() => {
            /**
             * @brief Fetches user data if the user is logged in and data needs to be updated.
             */
            const getUserData = async () => {
                try {
                    const response = await fetch(`${API_URL}/user/get`,
                        {
                            method: "GET",
                            credentials: "include",
                            headers: {
                                "Content-Type": "application/json",
                            },
                        });

                    if (!response.ok) {
                        throw new Error("Failed to fetch user data");
                    }

                    const userData = await response.json();
                    setUser(userData);

                } catch (error) {
                }
            };

            if (isLoggedIn) {
                getUserData();
            }

        },
        [isLoggedIn, isUserUpdate]);


    return (
        <header className="Navbar">
            <Link to={"/"} className="logo">
                DGA<span>-Detector</span>
            </Link>
            {isLoggedIn &&
            (
                <nav>
                    {data.map((item, i) => {
            return (
              <div
                  key={i}
                  className={`LinkItem ${isLinkActive(item.url) ? "active" : ""}`}>
                  <Link to={item.url}>
                      {item.label} {item.img && <img src={item.img} alt=""/>}
                  </Link>
                  {item.dropdown &&
 (
                  <div className="DropDown">
                      {item.dropdown.map((dropdown, index) => {
                      return (
                        <div key={index}>
                            {dropdown.onclick
 ? (
                            <Link onClick={handleLogout} to={dropdown.url}>
                                {dropdown.label}
                            </Link>
                          )
 : (
                            <Link to={dropdown.url}>{dropdown.label}</Link>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
                </nav>
            )}
        </header>
    );
}
