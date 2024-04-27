/**
 * @file index.jsx
 *
 * @brief Renders the user profile component.
 *
 * This file contains the implementation of the Profile component, which displays user profile information and allows the user to update their profile data. It utilizes React and Formik for form handling and state management.
 *
 * The main functionalities of this component include:
 * - Displaying user profile information such as username, email, phone number, and profile photo.
 * - Providing a form for users to update their profile information.
 * - Handling form submission to update user profile data via API calls.
 * - Supporting subscription to email notifications.
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
import { Formik, Form, Field, ErrorMessage } from "formik";
import Button from "../Button";
import API_URL from "../../../config";
import { toast } from "react-toastify";

/**
 * Profile component.
 *
 * @param {boolean} isLoggedIn - Indicates if the user is logged in.
 * @param {boolean} isUserUpdate - Indicates if the user data needs to be updated.
 * @param {function} setIsUserUpdate - Function to set the update status of user data.
 * @returns {JSX.Element} - Profile component JSX markup.
 */
export default function Profile({ isLoggedIn, isUserUpdate, setIsUserUpdate }) {
    const [user, setUser] = useState({
        photoUrl: "",
        userName: "",
        email: "",
        phoneNumber: "",
        subscribedToNotifications: false
    });

    /**
     * Handles form submission to update user profile.
     *
     * @param {object} values - Form field values.
     * @param {object} formik - Formik form instance.
     */
    const onSubmit = async (values, { setSubmitting }) => {
        try {
            const response = await fetch(`${API_URL}/User/update`,
                {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: values.email || "",
                        userName: values.userName || "",
                        phoneNumber: values.phoneNumber || "",
                        photoUrl: values.photoUrl || "",
                        subscribedToNotifications: values.subscribedToNotifications || false,
                    }),
                });

            if (!response.ok) {
                throw new Error("Failed to update user data");
            }
            toast.success("Profile updated!");

        } catch (error) {
            toast.error(`Error: ${error.message}`);
        } finally {
            setIsUserUpdate(new Date());
            setSubmitting(false);
        }
    };

    useEffect(() => {
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
                    setUser({
                        email: userData.email,
                        userName: userData.displayUserName,
                        phoneNumber: userData.phoneNumber,
                        photoUrl: userData.photoUrl,
                        subscribedToNotifications: userData.subscribedToNotifications,
                    });

                } catch (error) {
                    toast.error(`Error: ${error.message}`);
                }
            };

            if (isLoggedIn) {
                getUserData();
            }

        },
        [isLoggedIn, isUserUpdate]);


    return (
        <section className="Content">
            <div className="Content-Body">
                <div className="Profile">
                    <img src={user.photoUrl || avatar} alt=""/>

                    <Formik key={JSON.stringify(user)} initialValues={user} onSubmit={onSubmit}>
                        {(formik) => (
                            <Form>
                                <div className="InputGroup">

                                    <div>
                                        <label className="label" htmlFor="userName">
                                            User Name *
                                        </label>
                                        <Field
                                            type="text"
                                            name="userName"
                                            placeholder="User Name"
                                            className="input"
                                            value={formik.values.userName || ""}
                                            required/>
                                        <ErrorMessage name="userName" component="small"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="email">
                                            Email *
                                        </label>
                                        <Field
                                            type="email"
                                            name="email"
                                            placeholder="Email"
                                            className="input"
                                            value={formik.values.email || ""}
                                            required
                                            disabled/>
                                        <ErrorMessage name="email" component="small"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="phoneNumber">
                                            Telephone Number
                                        </label>
                                        <Field
                                            type="text"
                                            name="phoneNumber"
                                            placeholder="Number"
                                            className="input"
                                            value={formik.values.phoneNumber || ""}/>
                                        <ErrorMessage name="phoneNumber" component="small"/>
                                    </div>
                                    <div>
                                        <label className="label" htmlFor="photoUrl">
                                            Profile Photo (as url) - <i>Optional</i>
                                        </label>
                                        <Field
                                            type="text"
                                            name="photoUrl"
                                            placeholder="URL"
                                            className="input"
                                            value={formik.values.photoUrl || ""}/>
                                        <ErrorMessage name="photoUrl" component="small"/>
                                    </div>

                                    <div className="check">
                                        <label className="label" htmlFor="subscribedToNotifications">
                                            Subscribe to get email notifications
                                        </label>
                                        <Field
                                            type="checkbox"
                                            name="subscribedToNotifications"
                                            id="notifications"/>
                                        <ErrorMessage name="subscribedToNotifications" component="small"/>
                                    </div>

                                    <hr/>
                                    <div>
                                        <Link to={"/ChangePassword"}>Change password</Link>
                                    </div>
                                    <Button type="submit" small>
                                        Save
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
