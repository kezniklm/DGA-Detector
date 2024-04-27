/**
 * @file index.jsx
 *
 * @brief Component for user registration.
 *
 * This file contains the implementation of the Register component, which is responsible for user registration functionality in a React application. It allows users to register with their email, password, and security question-answer pair.
 *
 * The main functionalities of this component include:
 * - Handling user input for registration fields.
 * - Validating user input for email, password, and security question-answer.
 * - Sending registration data to the API endpoint for registration.
 * - Redirecting the user to the login page upon successful registration.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import Button from "../Button";
import API_URL from "../../../config";
import { toast } from "react-toastify";
import "./style.css"

/**
 * Register component for user registration.
 * @param {Object} props - Component props.
 * @param {Function} props.handleLogin - Function to handle user login.
 * @returns {JSX.Element} JSX representation of the component.
 */
export default function Register({ handleLogin }) {
    const navigate = useNavigate();

    const [step, setStep] = useState("1");
    const [que, setQue] = useState("What was the name of your first animal?");

    /**
     * Handle change event for security question.
     * @param {Event} e - The change event.
     */
    const onChange = (e) => {
        setQue(e.target.value);
    };

    /**
     * Handle step change event.
     */
    const handleStep = () => {
        setStep(2);
    };

    /**
     * Validate form values.
     * @param {Object} values - Form values.
     * @returns {Object} Object containing validation errors.
     */
    const validateForm = (values) => {
        const errors = {};
        if (!values.email) {
            errors.email = "Email is required";
        } else if (!/\S+@\S+\.\S+/.test(values.email)) {
            errors.email = "Invalid email address";
        }
        if (!values.password) {
            errors.password = "Password is required";
        } else if (values.password.length < 6) {
            errors.password = "Password must be at least 6 characters";
        } else if (!/(?=.*[A-Z])/.test(values.password)) {
            errors.password = "Password must contain at least one uppercase letter";
        } else if (!/(?=.*[!@#$%^&*])/.test(values.password)) {
            errors.password = "Password must contain at least one symbol";
        } else if (!/(?=.*\d)/.test(values.password)) {
            errors.password = "Password must contain at least one number";
        }
        return errors;
    };

    /**
     * Handle form submission.
     * @param {Object} values - Form values.
     * @param {Object} formikBag - Formik bag containing helpers.
     */
    const onSubmit = async (values, { setSubmitting, resetForm }) => {
        try {
            const response = await fetch(`${API_URL}/register`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(values),
                });

            if (!response.ok) {
                if (response.status === 400) {
                    const data = await response.text();
                    if (data.includes("already taken")) {
                        throw new Error("Email is already taken");
                    }
                }
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            sessionStorage.setItem("user", JSON.stringify(values));
        } catch (error) {
            toast.error(`Error: ${error.message}`);
            return;
        } finally {
            setSubmitting(false);
        }

        const baseLoginUrl = "https://localhost:7268/login";
        const queryParams = new URLSearchParams({
            useCookies: "false",
            useSessionCookies: "true",
        });
        const loginUrl = `${baseLoginUrl}?${queryParams}`;

        try {
            const response = await fetch(loginUrl,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(values),
                    credentials: "include",
                });

            if (!response.ok) {
                throw new Error("Login failed");
            }

            handleLogin(response.data);
            setStep("2");
        } catch (error) {
            toast.error(`Error: ${error.message}`);
            return;
        } finally {
            setSubmitting(false);
        }

        try {
            const response = await fetch(`${API_URL}/User/update`,
                {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: values.email,
                        userName: values.email,
                        phoneNumber: "",
                        photoUrl: "",
                        subscribedToNotifications: false,
                        securityQuestion: values.securityQuestion,
                        securityAnswer: values.securityAnswer,
                    }),
                });

            if (!response.ok) {
                throw new Error("Failed to add security question");
            }

            toast.success("Registered!");
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
                <div className="Form register">
                    <Formik
                        initialValues={{
              email: "",
              password: "",
              securityQuestion: "What was the name of your first animal?",
              securityAnswer: "",
            }}
                        validate={validateForm}
                        onSubmit={onSubmit}>
                        {() => (
                            <Form>
                                <div className="InputGroup">
                                    <h2 className="FormHeading">Register</h2>

                                    {step === "1"
 ? (
                    <>
<div>
    <label className="label" htmlFor="email">
        Email*
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
    <label className="label" htmlFor="password">
        Password*
    </label>
    <Field
        type="password"
        name="password"
        placeholder="Password"
        className="input"
        required/>
    <ErrorMessage name="password" component="small"/>
</div>

<div>
    <Link to={"/Login"}>Login?</Link>
</div>
<br/>
<Button type="button" onClick={handleStep} small>
    Next
</Button>
</>
                  )
 : (
                    <>
<div>
    <label className="label" htmlFor="securityQuestion">
        Security Question*
    </label>
    <Field
        as="select"
        name="securityQuestion"
        className="input"
        required
        value={que}
        onChange={onChange}>
        <option
            value={"What was the name of your first animal?"}>
            What was the name of your first animal?
        </option>
        <option
            value={"What was your favourite subject on school?"}>
            What was your favourite subject on school?
        </option>
        <option value={"What was your favourite car?"}>
            What was your favourite car?
        </option>
    </Field>
    <ErrorMessage
        name="securityQuestion"
        component="small"/>
</div>

<div>
    <label className="label" htmlFor="securityAnswer">
        Security Answer*
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
    <Link to={"/Login"}>Login?</Link>
</div>
<br/>
<Button type="submit" small>
    Submit
</Button>

</>
                  )}

                                </div>
                            </Form>
                        )}
                    </Formik>
                </div>
            </div>
        </section>
    );
}
