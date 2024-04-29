/**
 * @file index.jsx
 *
 * @brief Provides functionality for resetting password.
 *
 * This file contains the implementation of the ForgotPassword component, which allows users to reset their password. It includes form inputs for entering email, security question answer, and new password.
 *
 * The main functionalities of this component include:
 * - Sending a request to retrieve security question based on provided email.
 * - Validating user input for new password.
 * - Sending a request to reset password with the provided information.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useState } from "react";
import "./style.css";
import { Link, useNavigate } from "react-router-dom";
import Button from "../Button";
import { Formik, Form, Field, ErrorMessage } from "formik";
import API_URL from "../../../config";
import { toast } from "react-toastify";

/**
 * @brief Component for handling password recovery process.
 *
 * @returns JSX element representing the ForgotPassword component.
 */
export default function ForgotPassword() {
    const navigate = useNavigate();
    const initialValues = { email: "" };
    const [step, setStep] = useState("1");
    const [ques, setQues] = useState("");
    const [email, setEmail] = useState("");

    /**
     * @brief Changes the step of the password recovery process.
     */
    const handleStep = () => {
        setStep("2");
    };

    /**
     * @brief Submits the email address to retrieve security question.
     *
     * @param {object} values - Form values containing the email address.
     * @param {Function} setSubmitting - Function to set the submitting state.
     * @param {Function} resetForm - Function to reset the form.
     */
    const onSubmit = async (values, { setSubmitting, resetForm }) => {
        try {
            const response = await fetch(
                `${API_URL}/Public/SecurityQuestion/${values.email}`,
                {
                    method: "GET",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                }
            );

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error("Question not found");
                }
                throw new Error(`${response.status}`);
            }

            const data = await response.text();
            setStep("2");
            setQues(data);
            setEmail(values.email);

        } catch (error) {
            toast.error(`Error: ${error.message}`);
        } finally {
            setSubmitting(false);
        }
    };

    /**
     * @brief Validates the form values for setting new password.
     *
     * @param {object} values - Form values containing the new password.
     * @returns {object} - Object containing validation errors.
     */
    const validateForm = (values) => {
        const errors = {};

        if (!values.newPassword) {
            errors.newPassword = "Password is required";
        } else if (values.newPassword.length < 6) {
            errors.newPassword = "Password must be at least 6 characters";
        } else if (!/(?=.*[A-Z])/.test(values.newPassword)) {
            errors.newPassword =
                "Password must contain at least one uppercase letter";
        } else if (!/(?=.*[!@#$%^&*])/.test(values.newPassword)) {
            errors.newPassword = "Password must contain at least one symbol";
        } else if (!/(?=.*\d)/.test(values.newPassword)) {
            errors.newPassword = "Password must contain at least one number";
        }
        return errors;
    };

    /**
     * @brief Submits the new password and security answer to reset password.
     *
     * @param {object} values - Form values containing the new password and security answer.
     * @param {Function} setSubmitting - Function to set the submitting state.
     * @param {Function} resetForm - Function to reset the form.
     */
    const onSubmit2 = async (values, { setSubmitting, resetForm }) => {
        try {
            const response = await fetch(`${API_URL}/Public/reset-password`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: email,
                        newPassword: values.newPassword,
                        securityQuestion: ques,
                        securityAnswer: values.securityAnswer,
                    }),
                });

            if (!response.ok) {
                if (response.status === 400) {
                    throw new Error("Wrong input");
                }
                throw new Error(`${response.status}`);
            }

            toast.success("Password Changed!");
            navigate("/Login");

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
                    {step === "1" &&
                    (
                        <Formik initialValues={{ email: "" }} onSubmit={onSubmit}>
                            {() => (
                <Form>
                    <div className="InputGroup">
                        <h2 className="FormHeading">Forgot Password</h2>
                        <div>
                            <label
                                className="label bigLabel"
                                htmlFor="currentPassword">
                                Enter your email
                            </label>
                            <Field
                                type="email"
                                name="email"
                                placeholder="Email"
                                className="input"
                                required/>
                            <ErrorMessage name="email" component="small"/>
                        </div>

                        <div>
                            <Link to={"/Login"}>Login?</Link>
                        </div>

                        <br/>
                        <Button type="submit" small>
                            Submit
                        </Button>
                    </div>
                </Form>
              )}
                        </Formik>
                    )}
                    {step === "2" &&
                    (
                        <Formik
                            initialValues={{ securityAnswer: "", newPassword: "" }}
                            onSubmit={onSubmit2}
                            validate={validateForm}>
                            {() => (
                <Form>
                    <div className="InputGroup">
                        <h2 className="FormHeading">Enter your details</h2>
                        <div>
                            <label
                                className="label bigLabel"
                                htmlFor="securityAnswer">
                                {ques}
                            </label>
                            <Field
                                type="text"
                                name="securityAnswer"
                                placeholder="Answer"
                                className="input"
                                required/>
                            <ErrorMessage name="securityAnswer" component="small"/>
                        </div>

                        <div>
                            <label className="label bigLabel" htmlFor="newPassword">
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
                            <Link to={"/Login"}>Login?</Link>
                        </div>

                        <br/>
                        <Button type="submit" small>
                            Submit
                        </Button>
                    </div>
                </Form>
              )}
                        </Formik>
                    )}
                </div>
            </div>
        </section>
    );
}
