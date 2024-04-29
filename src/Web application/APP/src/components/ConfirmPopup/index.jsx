/**
 * @file index.js
 *
 * @brief Defines a component for displaying a confirmation popup.
 *
 * This file contains the implementation of the ConfirmPopup component, which is used to display a confirmation popup with a message and options to confirm or cancel an action.
 *
 * The main functionalities of this file include:
 * - Rendering a confirmation popup with a message.
 * - Providing buttons for confirming or canceling an action.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import "./style.css";
import Button from "../Button";

/**
 * Functional component representing a confirmation popup.
 *
 * @param {Object} props - The properties passed to the ConfirmPopup component.
 * @param {string} props.message - The message to be displayed in the confirmation popup.
 * @param {Function} props.onConfirm - The function to be called when the confirm button is clicked.
 * @param {Function} props.onCancel - The function to be called when the cancel button is clicked.
 * @returns {JSX.Element} A JSX element representing the confirmation popup.
 */
export default function ConfirmPopup({ message, onConfirm, onCancel }) {
    return (
        <div className="UpdatePage">
            <div className="Form">
                <div className="InputGroup">
                    <h2 className="PopupHeading">Confirm</h2>
                    <p className="PopupSubheading">{message}</p>
                    <div className="BtnGroups">
                        <Button type="button" onClick={onConfirm} small trans>
                            Confirm
                        </Button>
                        <Button warning onClick={onCancel} type="button" small trans>
                            Cancel
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
}
