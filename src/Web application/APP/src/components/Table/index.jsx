/**
 * @file index.jsx
 *
 * @brief React component for displaying data in a tabular format.
 *
 * This file contains the implementation of the Table component, which is used to display data in a tabular format in a React application.
 *
 * The main functionalities of this component include:
 * - Displaying data in a table based on the provided props.
 * - Sorting columns in ascending or descending order.
 * - Handling row update, delete, and move actions.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useEffect, useState } from "react";
import "./style.css";

/**
 * Table component for displaying data in a tabular format.
 * @param {Object} props - Component props.
 * @param {string} props.type - Type of table data (result, blacklist, whitelist).
 * @param {Array<Object>} props.data - Array of data objects to be displayed in the table.
 * @param {boolean} [props.result=true] - Indicates whether the table is displaying result data.
 * @param {function} props.onDelete - Callback function triggered when a row is deleted.
 * @param {function} props.onUpdate - Callback function triggered when a row is updated.
 * @param {function} props.onMove - Callback function triggered when a row is moved.
 * @param {function} props.onMoveR - Callback function triggered when a row is moved with a specified type.
 * @returns {JSX.Element} JSX element representing the table.
 */
export default function Table({
    type,
    data,
    result = true,
    onDelete,
    onUpdate,
    onMove,
    onMoveR,
}) {
    /**
     * State for storing sorted data.
     */
    const [sortedData, setSortedData] = useState([]);

    /**
     * State for storing the current sort order.
     */
    const [sortOrder, setSortOrder] = useState({
        column: 1,
        direction: "desc",
    });

    /**
     * Effect hook to update sorted data when the input data changes.
     */
    useEffect(() => {
            setSortedData(data);
        },
        [data]);

    /**
     * Handles the update action for a row.
     * @param {number} index - Index of the row to be updated.
     */
    const handleUpdate = (index) => {
        onUpdate(index);
    };

    /**
     * Handles the delete action for a row.
     * @param {number} index - Index of the row to be deleted.
     */
    const handleDelete = (index) => {
        const selectedItem = sortedData[index];
        onDelete(selectedItem.id);
    };

    /**
     * Handles the move action for a row.
     * @param {number} index - Index of the row to be moved.
     */
    const handleMove = (index) => {
        const selectedItem = sortedData[index];
        onMove(selectedItem.id);
    };

    /**
     * Handles the move with type action for a row.
     * @param {number} index - Index of the row to be moved.
     * @param {string} type - Type of move action (WL, BL).
     */
    const handleMoveR = (index, type) => {
        const selectedItem = sortedData[index];
        onMoveR(selectedItem.id, type);
    };

    /**
     * Handles sorting of the table columns.
     * @param {number} columnIndex - Index of the column to be sorted.
     * @param {string} columnType - Type of the column data (domainName, detected, etc.).
     */
    const handleSort = (columnIndex, columnType) => {
        const currentSortOrder =
            sortOrder.column === columnIndex
                ? sortOrder.direction === "asc"
                ? "desc"
                : "asc"
                : "asc";
        setSortOrder({ column: columnIndex, direction: currentSortOrder });

        const sorted = sortedData.slice().sort((a, b) => {
            switch (columnType) {
            case "didBlacklistHit":
                return currentSortOrder === "asc"
                    ? a[columnType] - b[columnType]
                    : b[columnType] - a[columnType];
            case "domainName":
                return currentSortOrder === "asc"
                    ? (a[columnType] || "").localeCompare(b[columnType] || "")
                    : (b[columnType] || "").localeCompare(a[columnType] || "");
            case "detected":
                return currentSortOrder === "asc"
                    ? (new Date(a[columnType]) || 0) - (new Date(b[columnType]) || 0)
                    : (new Date(b[columnType]) || 0) - (new Date(a[columnType]) || 0);
            case "added":
                return currentSortOrder === "asc"
                    ? (new Date(a[columnType]) || 0) - (new Date(b[columnType]) || 0)
                    : (new Date(b[columnType]) || 0) - (new Date(a[columnType]) || 0);
            case "dangerousProbabilityValue":
                const numA = a[columnType] || "";
                const numB = b[columnType] || "";
                return currentSortOrder === "asc" ? numA - numB : numB - numA;
            default:
                return 0;
            }
        });

        setSortedData(sorted);
    };

    /**
     * Gets the sort indicator for a column.
     * @param {number} columnIndex - Index of the column.
     * @returns {string} Sort indicator (▲ for ascending, ▼ for descending).
     */
    const getSortIndicator = (columnIndex) => {
        if (sortOrder.column === columnIndex) {
            return sortOrder.direction === "asc" ? "▲" : "▼";
        }
        return null;
    };

    return (
        <div className="customTableWrapper">
            <div className="BodyWrapper">
                <table id="customTable" className="display customTable">
                    <thead>
                    {result
                        ? (
                            <tr>
                                <th onClick={() => handleSort(0, "domainName")}>
                                    Domain Name <span className="dir">{getSortIndicator(0)}</span>
                                </th>
                                <th onClick={() => handleSort(1, "detected")}>
                                    Detected <span className="dir">{getSortIndicator(1)}</span>
                                </th>

                                <th onClick={() => handleSort(2, "dangerousProbabilityValue")}>
                                    Dangerous by %{" "}
                                    <span className="dir">{getSortIndicator(2)}</span>
                                </th>
                                <th onClick={() => handleSort(3, "didBlacklistHit")}>
                                    Filtered in Blacklist{" "}
                                    <span className="dir">{getSortIndicator(3)}</span>
                                </th>

                                <th className="bigButton"></th>
                                <th className="smallButton"></th>
                                <th className="smallButton"></th>
                            </tr>
                        )
                        : (
                            <tr>
                                <th onClick={() => handleSort(0, "domainName")}>
                                    Domain Name <span className="dir">{getSortIndicator(0)}</span>
                                </th>
                                <th onClick={() => handleSort(1, "added")}>
                                    Added <span className="dir">{getSortIndicator(1)}</span>
                                </th>
                                <th className="bigButton"></th>
                                <th className="smallButton"></th>
                                <th className="smallButton"></th>
                            </tr>
                        )}
                    </thead>
                    <tbody>
                    {sortedData.length > 0
                        ? (
                            sortedData.map((item, index) => (
                                <tr key={index}>
                                    <td>{item.domainName}</td>
                                    {result
 ? (
                    <td>{new Date(item.detected).toLocaleString()}</td>
                  )
 : (
                    <td>{new Date(item.added).toLocaleString()}</td>
                  )}
                                    {result && <td>{item.dangerousProbabilityValue}%</td>}
                                    {result && <td>{item.didBlacklistHit ? "true" : "false"}</td>}
                                    {type === "result" &&
 (
                    <td className="bigButton">
                        <button
                            onClick={() => handleMoveR(index, "WL")}
                            className="MoveBtn">
                            Move to WL
                        </button>{" "}
                        <button
                            onClick={() => handleMoveR(index, "BL")}
                            className="MoveBtn">
                            Move to BL
                        </button>{" "}
                    </td>
                  )}
                                    {type === "blacklist" &&
 (
                    <td className="bigButton">
                        <button
                            onClick={() => handleMove(index)}
                            className="MoveBtn">
                            Move to Whitelist
                        </button>{" "}
                    </td>
                  )}
                                    {type === "whitelist" &&
 (
                    <td className="bigButton">
                        <button
                            onClick={() => handleMove(index)}
                            className="MoveBtn">
                            Move to Blacklist
                        </button>{" "}
                    </td>
                  )}
                                    <td className="smallButton">
                                        <button
                                            onClick={() => handleUpdate(index)}
                                            className="UpdateBtn">
                                            Update
                                        </button>
                                    </td>
                                    <td className="smallButton">
                                        <button
                                            onClick={() => handleDelete(index)}
                                            className="DeleteBtn">
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))
                        )
                        : (
                            <tr>
                                <td style={{ textAlign: "center" }} colSpan="7">
                                    No data to show
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
