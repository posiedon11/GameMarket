
//import reactLogo from './assets/react.svg'
//import viteLogo from '/vite.svg'
import {
    createBrowserRouter,
    RouterProvider,
} from "react-router-dom";
import GameList from './GameList/GameList'
import GameDetail from './GameDetails/GameDetail'
import './App.css'
import NotFound from './NotFound'
import MainLayout from './GameList/MainLayout'
import FiltersProvider from './FiltersProvider';
//import { FiltersContext } from './FiltersContext';


const router = createBrowserRouter([
    {
        path: "/",
        element: <MainLayout><GameList /></MainLayout>,
    },
    {
        path: "/Game/:gameID",
        element: <GameDetail />,
    },
    {
        path: "*",
        element: <NotFound />
    }
]
);
function App() {
    try {
        //const {filters, setFilters} = useState({});
        return (
            <div>
                <ul>
                <li><a href="/">Home</a></li>
                </ul>
                <FiltersProvider>
                    <RouterProvider router={router} />
                </FiltersProvider>
            </div>
            

        );
    }
    catch (error) { console.error('Failed to fetch games:', error); }
}

export default App
