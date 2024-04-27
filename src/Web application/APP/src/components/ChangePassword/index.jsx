/**
 * @file index.jsx
 *
 * @brief Component for changing user password.
 *
 * This file contains the implementation of the ChangePassword component, which allows users to change their password through a form. It utilizes React, Formik for form handling, and communicates with the backend API to update the password securely.
 *
 * The main functionalities of this component include:
 * - Rendering a form with fields for current password, new password, and confirm password.
 * - Validating the form fields for password requirements.
 * - Sending a request to the backend API to change the user's password.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React from "react";
import "./style.css";
import { Link, useNavigate } from "react-router-dom";
import Button from "../Button";
import { Formik, Form, Field, ErrorMessage } from "formik";
import API_URL from "../../../config";
import { toast } from "react-toastify";

/**
 * @brief ChangePassword component allows users to change their password.
 *
 * @param {Object} props - Component props.
 * @param {boolean} props.isLoggedIn - Indicates whether the user is logged in.
 * @param {Function} props.handleLogin - Function to handle user login state.
 * @returns {JSX.Element} - ChangePassword component.
 */
export default function ChangePassword({ isLoggedIn, handleLogin }) {

    const navigate = useNavigate();

    const initialValues = { currentPassword: "", newPassword: "", confirmPassword: "" };

    /**
     * @brief Handles form submission.
     *
     * @param {Object} values - Form values.
     * @param {Function} formActions - Formik form actions.
     * @returns {void}
     */
    const onSubmit = async (values, { setSubmitting, resetForm }) => {
        if (values.newPassword !== values.confirmPassword) {
            toast.error(`Please confirm password!`);
        } else {
            try {
                const response = await fetch(`${API_URL}/User/change-password`,
                    {
                        method: "POST",
                        credentials: "include",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify(
                            {
                                "currentPassword": values.currentPassword,
                                "newPassword": values.newPassword
                            }
                        ),
                    });

                if (!response.ok) {
                    throw new Error(`${response.status}`);
                }
                resetForm();
                toast.success("Password updated!");
                navigate("/Profile");

            } catch (error) {
                toast.error(`Error: ${error.message}`);
            } finally {
                setSubmitting(false);
            }
        }

    };

    /**
     * @brief Validates form input.
     *
     * @param {Object} values - Form values.
     * @returns {Object} - Object containing validation errors.
     */
    const validateForm = (values) => {
        const errors = {};

        if (values.newPassword.length < 6) {
            errors.newPassword = "Password must be at least 6 characters";
        } else if (!/(?=.*[A-Z])/.test(values.newPassword)) {
            errors.newPassword = "Password must contain at least one uppercase letter";
        } else if (!/(?=.*[!@#$%^&*])/.test(values.newPassword)) {
            errors.newPassword = "Password must contain at least one symbol";
        } else if (!/(?=.*\d)/.test(values.newPassword)) {
            errors.newPassword = "Password must contain at least one number";
        }

        if (values.confirmPassword.length < 6) {
            errors.confirmPassword = "Password must be at least 6 characters";
        } else if (!/(?=.*[A-Z])/.test(values.confirmPassword)) {
            errors.confirmPassword = "Password must contain at least one uppercase letter";
        } else if (!/(?=.*[!@#$%^&*])/.test(values.confirmPassword)) {
            errors.confirmPassword = "Password must contain at least one symbol";
        } else if (!/(?=.*\d)/.test(values.confirmPassword)) {
            errors.confirmPassword = "Password must contain at least one number";
        }

        if (values.currentPassword.length < 6) {
            errors.currentPassword = "Password must be at least 6 characters";
        } else if (!/(?=.*[A-Z])/.test(values.currentPassword)) {
            errors.currentPassword = "Password must contain at least one uppercase letter";
        } else if (!/(?=.*[!@#$%^&*])/.test(values.currentPassword)) {
            errors.currentPassword = "Password must contain at least one symbol";
        } else if (!/(?=.*\d)/.test(values.currentPassword)) {
            errors.currentPassword = "Password must contain at least one number";
        }
        return errors;
    };

    return (
        <section className="Content">
            <div className="Content-Body">
                <div className="Form">
                    <Formik validate={validateForm} initialValues={initialValues} onSubmit={onSubmit}>
                        {() => (
                            <Form>
                                <div className="InputGroup">
                                    <h2 className="FormHeading">Change Passowrd</h2>
                                    <div>
                                        <label className="label" htmlFor="currentPassword">
                                            Current Password
                                        </label>
                                        <Field
                                            type="password"
                                            name="currentPassword"
                                            placeholder="Password"
                                            className="input"
                                            required/>
                                        <ErrorMessage name="currentPassword" component="small"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="newPassword">
                                            New Password
                                        </label>
                                        <Field
                                            type="password"
                                            name="newPassword"
                                            placeholder="Password"
                                            className="input"
                                            required/>
                                        <ErrorMessage name="newPassword" component="small"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="confirmPassword">
                                            Confirm Password
                                        </label>
                                        <Field
                                            type="password"
                                            name="confirmPassword"
                                            placeholder="Password"
                                            className="input"
                                            required/>
                                        <ErrorMessage name="confirmPassword" component="small"/>
                                    </div>
                                    <div>
                                        <Link to={"/Profile"}>Profile</Link>
                                    </div>

                                    <br/>
                                    <Button type="submit" small>
                                        Change
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
