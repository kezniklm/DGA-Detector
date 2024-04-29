/**
 * @file index.js
 *
 * @brief Provides functionality for user login.
 *
 * This file contains the implementation of the Login component, which allows users to log in to the application. It utilizes React and various libraries such as Formik for form management and react-toastify for displaying notifications.
 *
 * The main functionalities of this component include:
 * - Handling user login by sending a POST request to the login endpoint.
 * - Displaying a form for users to input their email and password.
 * - Providing links to register and reset password pages.
 * - Displaying success or error messages using toast notifications.
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
import { Link, useNavigate } from "react-router-dom";
import Button from "../Button";
import { Formik, Form, Field, ErrorMessage } from "formik";
import { toast } from "react-toastify";

/**
 * Login component for user authentication.
 *
 * @param {Object} props - Props for the Login component.
 * @param {Function} props.handleLogin - Function to handle user login.
 * @returns {JSX.Element} - Returns the JSX element representing the Login component.
 */
export default function Login({ handleLogin }) {
    const navigate = useNavigate();

    const baseLoginUrl = "https://localhost:7268/login";
    const queryParams = new URLSearchParams({
        useCookies: "false",
        useSessionCookies: "true"
    });


    const loginUrl = `${baseLoginUrl}?${queryParams}`;

    /**
     * Handles form submission for user login.
     *
     * @param {Object} values - Form values containing email and password.
     * @param {Function} setSubmitting - Function to set submitting state.
     * @returns {void}
     */
    const onSubmit = async (values, { setSubmitting }) => {
        try {
            const response = await fetch(loginUrl,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(values),
                    credentials: "include"
                });

            if (!response.ok) {
                throw new Error("Login failed");
            }

            handleLogin(response.data);
            toast.success("Logged In!");
            navigate("/");

        } catch (error) {
            toast.error(`Error: ${error.message}`);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <section className="Content">
            <div className="Content-Body">
                <div className="Form">
                    <Formik
                        initialValues={{ email: "", password: "" }}
                        onSubmit={onSubmit}>
                        {() => (
                            <Form>
                                <div className="InputGroup">
                                    <h2 className="FormHeading">Login</h2>
                                    <div>
                                        <label className="label" htmlFor="email">
                                            Email
                                        </label>
                                        <Field
                                            type="email"
                                            name="email"
                                            placeholder="Email"
                                            className="input"/>
                                        <ErrorMessage name="email" component="div"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="password">
                                            Password
                                        </label>
                                        <Field
                                            type="password"
                                            name="password"
                                            placeholder="Password"
                                            className="input"/>
                                        <ErrorMessage name="password" component="div"/>
                                    </div>
                                    <div className="flexBetween">
                                        <Link to={"/Register"}>Register?</Link>
                                        <Link to={"/ForgotPassword"}>Forgot Password?</Link>
                                    </div>

                                    <br/>
                                    <Button type="submit" small>
                                        Login
                                    </Button>
                                </div>
                            </Form>
                        )}
                    </Formik>
                </div>
            </div>
        </section>
    );
}
