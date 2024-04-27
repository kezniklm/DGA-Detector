/**
 * @file index.jsx
 * 
 * @brief Provides a React component for updating domain data.
 * 
 * This file contains the implementation of the `Update` functional component which renders a form to handle domain data updates.
 * It includes fields that are conditionally rendered based on the component's props. Utilizes Formik for form management.
 * 
 * The main functionalities of this file include:
 * - Rendering a form with inputs for domain name, dangerous probability, and blacklist status.
 * - Handling form submission with a simulated delay for asynchronous processing.
 * - Conditional display of form fields based on the 'result' prop.
 * 
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import "./style.css";
import Button from "../Button";
import { Formik, Form, Field, ErrorMessage } from "formik";

/**
 * @function Update
 * @brief Functional component for updating domain data.
 *
 * This component renders a form to update data about a domain. It conditionally
 * displays fields for dangerous probability and blacklist status based on the 'result' prop.
 *
 * @param {Object} props - The properties passed to the component.
 * @param {Object} props.data - Initial data for the form fields.
 * @param {function} props.handleUpdate - Function to call on form submission.
 * @param {function} props.handleUpdatePopup - Function to handle popup actions.
 * @param {boolean} [props.result=false] - Determines whether additional fields are shown.
 * @returns {JSX.Element} The rendered component.
 */
export default function Update({
    data,
    handleUpdate,
    handleUpdatePopup,
    result = false,
}) {
    /**
     * @brief Handles form submission with a delay.
     *
     * It sets a delay before calling the update handler to simulate asynchronous data processing.
     *
     * @param {Object} values - The form values.
     * @param {Object} formikBag - Formik helpers and state.
     */
    const onSubmit = (values, { setSubmitting }) => {
        setTimeout(() => {
                handleUpdate(values);
                setSubmitting(false);
            },
            400);
    };

    // Determine initial values based on result prop
    const initialValues = result
        ? {
            dangerousProbabilityValue: data.dangerousProbabilityValue || "",
            didBlacklistHit: data.didBlacklistHit || false,
            domainName: data.domainName || ""
        }
        : {
            domainName: data.domainName || ""
        };

    // Render the form with conditionally displayed fields
    return (
        <div className="UpdatePage">
            <div className="Form">
                <Formik initialValues={initialValues} onSubmit={onSubmit}>
                    {() => (
                        <Form>
                            <div className="InputGroup">
                                <h2 className="PopupHeading">Update</h2>

                                <div>
                                    <label className="label" htmlFor="domainName">
                                        Domain Name
                                    </label>
                                    <Field
                                        type="text"
                                        name="domainName"
                                        placeholder="Domain Name"
                                        className="input"
                                        required/>
                                    <ErrorMessage name="domainName" component="small"/>
                                </div>

                                {result &&
 (
                  <div>
                      <label
                          className="label"
                          htmlFor="dangerousProbabilityValue">
                          Dangerous by %
                      </label>
                      <Field
                          type="number"
                          name="dangerousProbabilityValue"
                          placeholder="Value"
                          className="input"
                          required
                          min="0"
                          max="100"/>
                      <ErrorMessage
                          name="dangerousProbabilityValue"
                          component="small"/>
                  </div>
                )}
                                {result &&
 (
                  <div className="check">
                      <label className="label" htmlFor="didBlacklistHit">
                          Did Blacklist Hit
                      </label>
                      <Field
                          type="checkbox"
                          name="didBlacklistHit"
                          placeholder="Filtered"
                          className="input"/>
                      <ErrorMessage name="didBlacklistHit" component="small"/>
                  </div>
                )}

                                <br/>
                                <div className="BtnGroups">
                                    <Button type="submit" small trans>
                                        Update
                                    </Button>
                                    <Button
                                        warning
                                        onClick={handleUpdatePopup}
                                        type="button"
                                        small
                                        trans>
                                        Cancel
                                    </Button>
                                </div>
                            </div>
                        </Form>
                    )}
                </Formik>
            </div>
        </div>
    );
}
