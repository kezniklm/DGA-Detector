/**
 * @file index.jsx
 *
 * @brief Component for adding data.
 *
 * This file contains the implementation of the Add component, which is responsible for adding data through a form. It utilizes Formik for form management and handles the submission of data.
 *
 * The main functionalities of this component include:
 * - Rendering a form to add data.
 * - Handling form submission.
 * - Displaying form validation errors.
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
 * Add component for adding data.
 *
 * @param {object} props - Component properties.
 * @param {function} props.handleAdd - Function to handle addition of data.
 * @param {function} props.handleAddClick - Function to handle click event for adding data.
 * @param {boolean} [props.result=false] - Flag indicating if result is available.
 * @returns {JSX.Element} JSX element representing the Add component.
 */
export default function Add({ handleAdd, handleAddClick, result = false }) {

    /**
     * Function to handle form submission.
     *
     * @param {object} values - Form values.
     * @param {object} formikBag - Formik bag.
     */
    const onSubmit = (values, { setSubmitting }) => {
        setTimeout(() => {
                handleAdd(values);
                setSubmitting(false);
            },
            400);
    };

    // Initialize form values based on result availability
    const initialValues =
        result ? { dangerousProbabilityValue: "", didBlacklistHit: false, domainName: "" } : { domainName: "" };

    return (
        <div className="UpdatePage">
            <div className="Form">
                <Formik initialValues={initialValues} onSubmit={onSubmit}>
                    {() => (
                        <Form>
                            <div className="InputGroup">
                                <h2 className="PopupHeading">Add</h2>

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
 <div>
     <label className="label" htmlFor="dangerousProbabilityValue">
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
     <ErrorMessage name="dangerousProbabilityValue" component="small"/>
 </div>}
                                <br/>
                                <div className="BtnGroups">
                                    <Button type="submit" small trans>
                                        Add
                                    </Button>
                                    <Button
                                        warning
                                        onClick={handleAddClick}
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
