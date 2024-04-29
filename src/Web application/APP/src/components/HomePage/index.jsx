/**
 * @file index.jsx
 *
 * @brief Functional component representing the home page.
 *
 * This file contains the implementation of the HomePage component, which serves as the landing page of the application. It displays relevant information and statistics about the application's functionality and provides options for user interaction.
 * 
 * The main functionalities of this file include:
 * - Fetching various statistics and data from the API endpoints.
 * - Rendering dynamic content based on fetched data.
 * - Animating page elements using GSAP library.
 * 
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useEffect, useState } from "react";
import "./style.css";
import homePic from "../../assets/logo.png";
import Button from "../Button/index.jsx";
import { Link } from "react-router-dom";
import gsap from "gsap";
import API_URL from "../../../config.js";

/**
 * @brief Functional component representing the home page.
 *
 * @param {object} props - Props passed to the component.
 * @param {boolean} props.isLoggedIn - Flag indicating whether the user is logged in.
 * @param {function} props.handleLogin - Function to handle user login.
 * @returns {JSX.Element} - Rendered home page component.
 */
export default function HomePage({ isLoggedIn, handleLogin }) {
  const [count, setCount] = useState({
    blacklist: 0,
    whitelist: 0,
  });
  const [domains, setDomains] = useState(0);
  const [positives, setPositives] = useState(0);
  const [filtered, setFiltered] = useState(0);

  /**
   * @brief Fetches the count of domains in the blacklist.
   */
  const fetchBlackCount = async () => {
    try {
      const response = await fetch(`${API_URL}/Public/blacklist/count`, {
        method: "GET",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error("Failed to fetch data");
      }
      const blacklistData = await response.json();
      setCount((prevState) => ({
        ...prevState,
        blacklist: blacklistData,
      }));
    } catch (error) {
    }
    };

    /**
   * @brief Fetches the count of domains in the whitelist.
   */
  const fetchWhiteCount = async () => {
    try {
      const response = await fetch(`${API_URL}/Public/whitelist/count`, {
        method: "GET",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error("Failed to fetch data");
      }
      const whitelistData = await response.json();
      setCount((prevState) => ({
        ...prevState,
        whitelist: whitelistData,
      }));
    } catch (error) {
    }
  };

  /**
   * @brief Fetches the number of domains detected today.
   */
  const fetchDomains = async () => {
    try {
      const response = await fetch(`${API_URL}/Public/NumberOfDomainsToday`, {
        method: "GET",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error("Failed to fetch data");
      }
      const data = await response.json();
      setDomains(data);
    } catch (error) {
    }
  };

  /**
   * @brief Fetches the number of positive results detected today.
   */
  const fetchPositives = async () => {
    try {
      const response = await fetch(`${API_URL}/Public/PositiveResultsToday`, {
        method: "GET",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error("Failed to fetch data");
      }
      const data = await response.json();
      setPositives(data);
    } catch (error) {
    }
  };

  /**
   * @brief Fetches the number of domains filtered by the blacklist today.
   */
  const fetchFiltered = async () => {
    try {
      const response = await fetch(`${API_URL}/Public/FilteredByBlacklist`, {
        method: "GET",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error("Failed to fetch data");
      }
      const data = await response.json();
      setFiltered(data);
    } catch (error) {
    }
  };

  useEffect(() => {
    gsap.fromTo(
      ".Homepage-footer > div",
      {
        opacity: 0,
        y: 100,
      },
      {
        y: 0,
        delay: 0.5,
        duration: 1,
        stagger: 0.3,
        opacity: 1,
      }
    );

    gsap.fromTo(
      [".Homepage-Body-detail > *", ".Homepage-Button"],
      {
        opacity: 0,
        x: -100,
      },
      {
        x: 0,
        delay: 1.5,
        duration: 1,
        stagger: 0.3,
        opacity: 1,
      }
    );

    gsap.fromTo(
      ".Homepage-Body-image",
      {
        opacity: 0,
        x: 100,
      },
      {
        x: 0,
        delay: 1.5,
        duration: 1,
        stagger: 0.3,
        opacity: 1,
      }
    );

    fetchDomains();
    fetchPositives();
    fetchFiltered();
    fetchWhiteCount();
    fetchBlackCount();
  }, []);

  return (
    <section className="Content">
        <div className="Homepage-Body Content-Body">
            <div className="Homepage-Body-detail">
                <h1>Empowering You in the Digital Age</h1>

                <p>
                    DGA-Detector is a cutting-edge detection system for DGA domains,
                    while empowering users to effectively monitor and manage classified
                    domains, ensuring a robust solution for identifying and handling
                    potential threats. With its intuitive interface and powerful
                    functionalities, DGA-Detector simplifies domain classification,
                    enabling users to swiftly address any anomalies or potential risks.
                </p>
            </div>
            <div className="Homepage-Body-image">
                <img src={homePic} alt=""/>
            </div>
        </div>
        <div className="Homepage-Button">
            {!isLoggedIn && (
          <Link to="/Login">
              <Button>Login or register to see detection results</Button>
          </Link>
        )}
        </div>

        <div className="Homepage-footer">
            <div>
                Number of domains today <br/> <b>{domains}</b>{" "}
            </div>
            <div>
                Number of positive results today <br/>{" "}
                <b>{positives}</b>{" "}
            </div>
            <div>
                Filtered by blacklist <br/> <b>{filtered}</b>{" "}
            </div>
            <div>
                Number of entries in <br/>
                Blacklist and Whitelist <br/>
                <b>{count.blacklist + count.whitelist}</b>
            </div>
        </div>
    </section>
  );
}
