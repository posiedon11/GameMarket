/* eslint-disable react/prop-types */
import { useState } from 'react';



const SearchBar = ({ data }) =>
{
    const [query, setQuery] = useState('');
    const handleSearch = (event) => {
        setQuery(event.target.value);
    };

    const filteredData = data.filter((item => item.name.toLowerCase().includes(query.toLowerCase())));

    return (
        <div>
            <input type="text" placeholder="Search..." value={query} onChange={handleSearch} />
            <ul>
                {filteredData.map((item) => (
                    <li key={item.id}>{item.name}</li>
                ))}
            </ul>
        </div>
    );
}
export default SearchBar;