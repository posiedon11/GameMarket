/* eslint-disable no-unused-vars */
// Sidebar.js
import FiltersContext from '../FiltersContext';
import { useContext } from 'react';

// eslint-disable-next-line react/prop-types
function Sidebar() {
    const { filters, setFilters } = useContext(FiltersContext);

    const handleFilterChange = (filterType, filterValue) => { setFilters(filterType, filterValue) };

    return (
        <div className="Filter-Main">
            <h2>Filters</h2>
            <div className="Filter-Group">
                <h4>Platform</h4>
                {["Xbox", "Steam"].map(platform => (
                    <label key={platform} className="Filter-Platform">
                        <input
                            type="checkbox"
                            checked={filters.platforms.includes(platform)}
                            onChange={(e) => handleFilterChange('platforms', platform)}
                        />
                        {platform}
                    </label>
                ))}
            </div>
            <div className="Filter-Group">
                <h4>Devices</h4>
                {["Xbox", "Xbox360", "XboxOne", "XboxSeries", "Windows", "Mac", "Linux"].map(device => (
                    <label key={device} className="Filter-Device">
                        <input
                            type="checkbox"
                            checked={filters.devices.includes(device)}
                            onChange={(e) => handleFilterChange('devices', device)}
                        />
                        {device}
                    </label>
                ))}
            </div>
            <div className="Filter-Group">
                <h4>Genres</h4>
                {["Action", "Adventure"].map(genre => (
                    <label key={genre} className="Filter-Genre">
                    <input
                            type="checkbox"
                            checked={filters.genres.includes(genre)}
                            onChange={(e) => handleFilterChange('genres', genre)}
                        />
                        {genre}
                    </label>
                )) }
            </div>
            {/* Repeat for other categories like Genre, Price, etc. */}
        </div>
    );
}

export default Sidebar;
