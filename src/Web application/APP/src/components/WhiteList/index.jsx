/**
 * @file index.jsx
 *
 * @brief Handles the management of whitelist entries.
 *
 * This file contains the implementation of the Whitelist component, which manages whitelist entries through CRUD operations. It uses React hooks for state management and handles user interactions for adding, updating, deleting, and moving entries between whitelists and blacklists.
 *
 * The main functionalities of this file include:
 * - Managing state with hooks for data, UI state, and pagination.
 * - Providing interactive UI elements for CRUD operations on whitelist entries.
 * - Fetching data from a server-side API and updating the UI based on these interactions.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useEffect, useLayoutEffect, useState } from "react";
import Table from "../Table/index.jsx";
import "./style.css";
import Update from "../Update/index.jsx";
import Add from "../Add/index.jsx";
import Button from "../Button/index.jsx";
import API_URL from "../../../config";
import { toast } from "react-toastify";
import ConfirmPopup from "../ConfirmPopup/index.jsx";

/**
 * @brief Represents the main Whitelist component that handles the whitelist management.
 * @details This component uses various hooks for state management and provides CRUD operations on whitelist items.
 */
export default function Whitelist() {
    // State hooks for component data and UI state
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState([]);
    const [addOpen, setAddOpen] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [showMoveConfirm, setShowMoveConfirm] = useState(false);
    const [idToDelete, setIdToDelete] = useState(null);
    const [iddToMove, setIdToMove] = useState(null);
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [filterName, setFilterName] = useState("");

    const [pageNum, setPageNum] = useState(1);
    const [maxNum, setMaxNum] = useState("10");

    const [updateOpen, setUpdateOpen] = useState({
        id: {},
        index: -1,
        open: false,
    });

    /**
     * @brief Toggles the update modal state and sets selected item data.
     * @param [in] index The index of the selected item.
     */
    const handleUpdateClick = (index = -1) => {
        const selectedItem = data[index];

        setUpdateOpen({
            id: selectedItem ? selectedItem.id : {},
            index: index,
            open: !updateOpen.open,
        });
    };

    /**
     * @brief Handles updating an item in the whitelist.
     * @param [in] values The new values to update the item with.
     */
    const handleUpdate = async (values) => {
        const currentDate = new Date().toISOString();
        try {
            await fetch(`${API_URL}/whitelist`,
                {
                    method: "PATCH",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        added: currentDate,
                        id: updateOpen.id,
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Updating: ${error.message}`);
        }
        handleUpdateClick();
    };

    /**
     * @brief Toggles the add modal state.
     */
    const handleAddClick = () => {
        setAddOpen(!addOpen);
    };

    /**
     * @brief Handles adding a new item to the whitelist.
     * @param [in] values The values for the new item.
     */
    const handleAdd = async (values) => {
        const currentDate = new Date().toISOString();
        try {
            await fetch(`${API_URL}/whitelist`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        added: currentDate,
                        id: "",
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Adding: ${error.message}`);
        }
        handleAddClick();
    };

    /**
     * @brief Fetches data from the server and updates the component state.
     */
    const fetchData = async () => {
        try {
            const response = await fetch(`${API_URL}/whitelist`,
                {
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
            setData(blacklistData);
        } catch (error) {
            toast.error(`Error: ${error.message}`);
        } finally {
            setLoading(false);
        }
    };

    /**
     * @brief Handles the deletion of an item from the whitelist.
     * @param [in] id The ID of the item to delete.
     */
    const handleDelete = async (id) => {
        setIdToDelete(id);
        setShowDeleteConfirm(true);
    };

    /**
     * @brief Confirms the deletion of an item.
     */
    const confirmDelete = async () => {
        try {
            await fetch(`${API_URL}/whitelist/${idToDelete}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Deleting: ${error.message}`);
        }
    };

    /**
     * @brief Cancels the deletion process.
     */
    const cancelDelete = () => {
        setShowDeleteConfirm(false);
    };

    /**
     * @brief Handles moving an item from the whitelist to the blacklist.
     * @param [in] id The ID of the item to move.
     */
    const handleMove = async (id) => {
        setIdToMove(id);
        setShowMoveConfirm(true);
    };

    /**
     * @brief Confirms the move of an item from the whitelist to the blacklist.
     */
    const confirmMove = async () => {
        const deletedItem = data.find((item) => item.id === iddToMove);
        const currentDate = new Date().toISOString();

        try {
            await fetch(`${API_URL}/whitelist/${iddToMove}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Moving: ${error.message}`);
        }

        try {
            await fetch(`${API_URL}/blacklist`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        domainName: deletedItem.domainName,
                        added: currentDate,
                        id: "",
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Adding: ${error.message}`);
        }
        setShowMoveConfirm(false);
    };

    /**
     * @brief Cancels the move process.
     */
    const cancelMove = () => {
        setShowMoveConfirm(false);
    };

    /**
     * @brief Handles changes to the start date filter input.
     * @details This function updates the start date state, which is used for filtering the displayed data.
     * @param event The event object containing the new start date value.
     */
    const handleStartDateChange = (event) => {
        setStartDate(event.target.value);
    };

    /**
     * @brief Handles changes to the maximum number of items to display per page.
     * @details This function updates the maximum number of items and resets the page number to 1, triggering a re-fetch of data with the new limits.
     * @param event The event object containing the new maximum number value.
     */
    const handleMaxChange = (event) => {
        setMaxNum(event.target.value);
        setPageNum(1);
    };

    /**
     * @brief Handles changes to the end date filter input.
     * @details This function updates the end date state, which is used for filtering the displayed data.
     * @param event The event object containing the new end date value.
     */
    const handleEndDateChange = (event) => {
        setEndDate(event.target.value);
    };

    /**
     * @brief Handles changes to the filter name input.
     * @details This function updates the filter name state, which is used for filtering the displayed data based on a text input.
     * @param event The event object containing the new filter name value.
     */
    const handleFilterName = (event) => {
        setFilterName(event.target.value);
    };

    /**
     * @brief Handles the pagination to the previous page.
     * @details Decrements the page number state if it's greater than 1, triggering a re-fetch of data with the updated page number.
     */
    const handlePagePrev = () => {
        if (pageNum > 1) {
            setPageNum(pageNum - 1);
        }
    };

    /**
     * @brief Handles the pagination to the next page.
     * @details Increments the page number state if there is data to show on the next page, triggering a re-fetch of data with the updated page number.
     */
    const handlePageNext = () => {
        if (data.length > 0) {
            setPageNum(pageNum + 1);
        }
    };

    /**
     * @brief Clears all filter inputs and resets pagination.
     * @details This function resets all filter states to their default values and sets the page number to 1, effectively clearing all filters and starting from the first page.
     */
    const clearFilter = () => {
        setFilterName("");
        setStartDate("");
        setEndDate("");
        setMaxNum("10");
        setPageNum(1);
    };

    /**
     * @brief Applies filters to the data query.
     */
    const handleFilter = async () => {
        const startDateValue = startDate && new Date(startDate).toISOString();
        const endDateValue = endDate && new Date(endDate).toISOString();

        var startDateStr = startDateValue ? `?startDate=${startDateValue}` : "";
        var endDateStr = endDateValue ? `&endDate=${endDateValue}` : "";

        const url = `${API_URL}/whitelist/${maxNum}/${pageNum}/${filterName}${startDateStr}${endDateStr}`;

        try {
            const response = await fetch(url,
                {
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
            setData(data);
        } catch (error) {
        } finally {
            setLoading(false);
        }
    };

    // Effect to refresh data on filters change
    useLayoutEffect(() => {
            handleFilter();
        },
        [startDate, endDate, filterName, maxNum, pageNum]);

    // Main component render
    return (
        <section className="Content">
            <div className="Content-Body">
                <div style={{ width: "100%" }}>
                    {loading
                        ? (
                            <div className="loading">Loading...</div>
                        )
                        : (
                            <>
<div className="topBar">
    <Button
        type="button"
        small
        onClick={handleAddClick}>
        Add Item +
    </Button>
    <div className="filterBar">
        <select
            onChange={handleMaxChange}
            value={maxNum}>
            <option value="1">1</option>
            <option value="10">10</option>
            <option value="20">20</option>
            <option value="50">50</option>
            <option value="100">100</option>
        </select>
        <input
            type="text"
            value={filterName}
            onChange={handleFilterName}
            placeholder="Search"/>
        <input
            type="datetime-local"
            className="datetime"
            value={startDate}
            onChange={handleStartDateChange}/>
        <input
            type="datetime-local"
            className="datetime"
            value={endDate}
            onChange={handleEndDateChange}/>
    </div>
    <div className="pageBar">
        <button
            onClick={handlePagePrev}
            disabled={pageNum <= 1}>
            {"<"}
        </button>
        <p>{pageNum}</p>
        <button
            onClick={handlePageNext}
            disabled={!(data.length > 0)}>
            {">"}
        </button>
    </div>
    <Button
        type="button"
        small
        trans
        onClick={clearFilter}>
        Clear
    </Button>
</div>
<Table
    data={data}
    result={false}
    onDelete={handleDelete}
    onUpdate={handleUpdateClick}
    onMove={handleMove}
    type={"whitelist"}/>
<p>
    Showing {Math.min(maxNum, data.length)} entries,
    on page {pageNum}{" "}
</p>
</>
                        )}
                </div>
                {updateOpen.open &&
                (
                    <Update
                        data={data[updateOpen.index]}
                        handleUpdatePopup={handleUpdateClick}
                        handleUpdate={handleUpdate}/>
                )}
                {addOpen &&
                (
                    <Add
                        handleAddClick={handleAddClick}
                        handleAdd={handleAdd}/>
                )}
                {showDeleteConfirm &&
                (
                    <ConfirmPopup
                        message="Are you sure you want to delete?"
                        onConfirm={confirmDelete}
                        onCancel={cancelDelete}/>
                )}
                {showMoveConfirm &&
                (
                    <ConfirmPopup
                        message="Are you sure you want to move to Blacklist?"
                        onConfirm={confirmMove}
                        onCancel={cancelMove}/>
                )}
            </div>
        </section>
    );
}
