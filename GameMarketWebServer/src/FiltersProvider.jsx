/* eslint-disable react/prop-types */
import FiltersContext from './FiltersContext';
import { useState } from 'react';

const FiltersProvider = ({ children }) => {
    const [filters, setFilters] = useState({
        platforms: [],
        devices: [],
        genres: []
    });

    const handleFilterChange = (filterType, filterValue) => {
        setFilters(prevFilters => ({
            ...prevFilters,
            [filterType]: prevFilters[filterType].includes(filterValue)
                ? prevFilters[filterType].filter(item => item !== filterValue)  // Toggle off
                : [...prevFilters[filterType], filterValue]  // Toggle on
        }));
    }

    // The value object that is passed to the provider
    const value = { filters, setFilters: handleFilterChange };

    return (
        <FiltersContext.Provider value={value}>
            {children}
        </FiltersContext.Provider>
    );
};

export default FiltersProvider;
