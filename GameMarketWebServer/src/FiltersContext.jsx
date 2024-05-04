import React from 'react';

const FiltersContext = React.createContext({
    filters: { platforms: [], devices:[], genres: [] },
    setFilters: () => { }
});

export default FiltersContext;