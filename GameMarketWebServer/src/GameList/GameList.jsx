import { useEffect, useState, useContext } from 'react';
import { Link } from 'react-router-dom';

import './GameList.css';
import xboxLogo from '../assets/Xbox-Logo.png';
import steamLogo from '../assets/Steam-Logo.png';
import FiltersContext from '../FiltersContext';
import DisplayGameTile from './DisplayGameTile';

const apiURL = import.meta.env.VITE_API_BASE_URL;

const GameList = () => {
    const [games, setGames] = useState([]); 
    const [loading, setLoading] = useState(true);
    const { filters } = useContext(FiltersContext); 
    const [search, setSearch] = useState("");

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                const query = new URLSearchParams({
                    platforms: filters.platforms.join(','),
                    genres: filters.genres.join(','),
                    devices: filters.devices.join(',')
                }).toString();
                const response = await fetch(`${apiURL}/MergedGamesList?${query}`);
                const data = await response.json();
                setGames(data);
            } catch (error) {
                console.error('Failed to fetch games:', error);
            }
            setLoading(false);
        };
        fetchData();
    }, [filters]); 

    const handleSearchChange = (event) => {
        setSearch(event.target.value); // Update the search state with the user input
    };

    const filteredGames = games.filter(game =>
        game.gameTitle.toLowerCase().includes(search.toLowerCase())
    );


    if (loading) {
        return <div>Loading...</div>; 
    }

    return (
        <div className="Game-Container">
            <input type="text" placeholder="Search..." value={search} onChange={handleSearchChange} />

            <div className="Game-List">
                {
                    filteredGames.map(game => (
                        <Link key={game.gameID} to={`/Game/${game.gameID}`} className="Game-Detail">
                            <DisplayGameTile game={game} />
                        </Link>
                    ))}
            </div>
        </div>
        
    );
    

};

export default GameList;
