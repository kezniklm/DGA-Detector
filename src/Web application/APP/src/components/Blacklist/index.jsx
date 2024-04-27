/**
 * @file index.jsx
 *
 * @brief Provides functionality for managing blacklist items in the UI.
 *
 * This file contains the implementation of the Blacklist component, which is responsible for rendering and managing the blacklist items in the user interface. It allows users to view, add, update, delete, and filter blacklist items.
 *
 * The main functionalities of this component include:
 * - Rendering a table of blacklist items.
 * - Providing options to add, update, and delete blacklist items.
 * - Implementing filtering functionality based on various criteria such as name and date.
 * - Handling pagination for displaying a limited number of items per page.
 * - Utilizing API calls to interact with the backend server.
 * - Displaying loading indicators while fetching data.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useLayoutEffect, useState } from "react";
import Table from "../Table/index.jsx";
import "./style.css";
import Update from "../Update/index.jsx";
import Add from "../Add/index.jsx";
import Button from "../Button/index.jsx";
import API_URL from "../../../config";
import ConfirmPopup from "../ConfirmPopup/index.jsx";
import { toast } from "react-toastify";

/**
 * @brief Component representing the Blacklist management.
 * @return {JSX.Element} JSX for the Blacklist component.
 */
export default function Blacklist() {
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
     * @brief Handles the click event for updating an item.
     * @param {number} index - Index of the item to update.
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
     * @brief Handles the update operation for an item.
     * @param {Object} values - Updated values for the item.
     */
    const handleUpdate = async (values) => {
        const currentDate = new Date().toISOString();
        try {
            await fetch(`${API_URL}/blacklist`,
                {
                    method: "PATCH",
                    credentials: "include",
                    headers: {
                        'Content-Type': "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        added: currentDate,
                        id: updateOpen.id
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Updating: ${error.message}`);
        }
        handleUpdateClick();
    };

    /**
     * @brief Handles the click event for adding an item.
     */
    const handleAddClick = () => {
        setAddOpen(!addOpen);
    };

    /**
     * @brief Handles the add operation for an item.
     * @param {Object} values - Values of the item to be added.
     */
    const handleAdd = async (values) => {
        const currentDate = new Date().toISOString();
        try {
            await fetch(`${API_URL}/blacklist`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        'Content-Type': "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        added: currentDate,
                        id: ""
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Adding: ${error.message}`);
        }
        handleAddClick();
    };

    /**
     * @brief Fetches data for the blacklist.
     */
    const fetchData = async () => {
        try {
            const response = await fetch(`${API_URL}/blacklist`,
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
     * Handles the click event for deleting an item.
     * @param {string} id - ID of the item to be deleted.
     */
    const handleDelete = async (id) => {
        setIdToDelete(id);
        setShowDeleteConfirm(true);
    };

    /**
     * Confirms the deletion of an item.
     */
    const confirmDelete = async () => {
        try {
            await fetch(`${API_URL}/blacklist/${idToDelete}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        'Content-Type': "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Deleting: ${error.message}`);
        }
    };

    /**
     * Cancels the deletion of an item.
     */
    const cancelDelete = () => {
        setShowDeleteConfirm(false);
    };

    /**
     * Handles the click event for moving an item.
     * @param {string} id - ID of the item to be moved.
     */
    const handleMove = async (id) => {
        setIdToMove(id);
        setShowMoveConfirm(true);
    };

    /**
     * Confirms the move of an item.
     */
    const confirmMove = async () => {
        const deletedItem = data.find(item => item.id === iddToMove);
        const currentDate = new Date().toISOString();

        try {
            await fetch(`${API_URL}/blacklist/${iddToMove}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        'Content-Type': "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Moving: ${error.message}`);
        }

        try {
            await fetch(`${API_URL}/whitelist`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        'Content-Type': "application/json",
                    },
                    body: JSON.stringify({
                        domainName: deletedItem.domainName,
                        added: currentDate,
                        id: ""
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Adding: ${error.message}`);
        }
        setShowMoveConfirm(false);

    };

    /**
     * Cancels the move operation.
     */
    const cancelMove = () => {
        setShowMoveConfirm(false);
    };

    /**
     * Handles the change event for the start date input.
     * @param {Event} event - The input change event.
     */
    const handleStartDateChange = (event) => {
        setStartDate(event.target.value);
    };

    /**
     * Handles the change event for the maximum number input.
     * @param {Event} event - The input change event.
     */
    const handleMaxChange = (event) => {
        setMaxNum(event.target.value);
        setPageNum(1);
    };

    /**
     * Handles the change event for the end date input.
     * @param {Event} event - The input change event.
     */
    const handleEndDateChange = (event) => {
        setEndDate(event.target.value);
    };

    /**
     * Handles the change event for the filter name input.
     * @param {Event} event - The input change event.
     */
    const handleFilterName = (event) => {
        setFilterName(event.target.value);
    };

    /**
     * Handles the click event for navigating to the previous page.
     */
    const handlePagePrev = () => {
        if (pageNum > 1) {
            setPageNum(pageNum - 1);
        }
    };

    /**
     * Handles the click event for navigating to the next page.
     */
    const handlePageNext = () => {
        if (data.length > 0) {
            setPageNum(pageNum + 1);
        }
    };

    /**
     * Clears all filters.
     */
    const clearFilter = () => {
        setFilterName("");
        setStartDate("");
        setEndDate("");
        setMaxNum("10");
        setPageNum(1);
    };

    /**
     * Handles the filter operation.
     */
    const handleFilter = async () => {
        const startDateValue = startDate && new Date(startDate).toISOString();
        const endDateValue = endDate && new Date(endDate).toISOString();


        var startDateStr = startDateValue ? `?startDate=${startDateValue}` : "";
        var endDateStr = endDateValue ? `&endDate=${endDateValue}` : "";

        const url = `${API_URL}/blacklist/${maxNum}/${pageNum}/${filterName}${startDateStr}${endDateStr}`;

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

    /**
     * Effect hook to fetch data and apply filters on component mount or when filters change.
     */
    useLayoutEffect(() => {
            handleFilter();
        },
        [startDate, endDate, filterName, maxNum, pageNum]);

    /**
     * Renders the Blacklist component.
     */
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
                 <Button type="button" small onClick={handleAddClick}>
                     Add Item +
                 </Button>
                 <div className="filterBar">
                     <select onChange={handleMaxChange} value={maxNum}>
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
                     <button onClick={handlePagePrev} disabled={pageNum <= 1}>{"<"}</button>
                     <p>{pageNum}</p>
                     <button onClick={handlePageNext} disabled={!(data.length > 0)}>{">"}</button>
                 </div>
                 <Button type="button" small trans onClick={clearFilter}>
                     Clear
                 </Button>
             </div>
<Table
                  data={data}
                  result={false}
                  onDelete={handleDelete}
                  onUpdate={handleUpdateClick}
                  onMove={handleMove}
                  type={"blacklist"}/>
<p>Showing {Math.min(maxNum, data.length)} entries, on page {pageNum} </p>
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
          message="Are you sure you want to move to Whitelist?"
          onConfirm={confirmMove}
          onCancel={cancelMove}
        />
                )}
            </div>
        </section>
    );
}
