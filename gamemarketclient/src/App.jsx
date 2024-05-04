
//import reactLogo from './assets/react.svg'
//import viteLogo from '/vite.svg'
import { BrowserRouter as Router, Route } from 'react-router-dom';
import GameList from './GameList'
import GameDetail from './GameDetail'
import  Routes from '../node_modules/react-router-dom/dist/index';


//function App() {
//    const [count, setCount] = useState(0)

//    return (
//        <>

//            <div>

//                <a href="https://vitejs.dev" target="_blank">
//                    <img src={viteLogo} className="logo" alt="Vite logo" />
//                </a>
//                <a href="https://react.dev" target="_blank">
//                    <img src={reactLogo} className="logo react" alt="React logo" />
//                </a>
//            </div>
//            <h1>Vite + React</h1>

//            <h1>Thing2</h1>
//            <div className="card">
//                <ThingPage/>
//                <button onClick={() => setCount((count) => count + 1)}>
//                    count is {count}
//                </button>
//                <p>
//                    Edit <code>src/App.jsx</code> and save to test HMR
//                </p>
//            </div>
//            <p className="read-the-docs">
//                Click on the Vite and React logos to learn more
//            </p>
//        </>
//    )
//}


export default function App() {
    try {


        return (
            <Router>
                <switch>
                    <Routes>
                        <Route path="/" exact component={GameList} />
                        <Route path="/Game/:gameID" component={GameDetail} />
                    </Routes>
                </switch>
            </Router>
        );
    }
    catch (error) { console.error('Failed to fetch games:', error); }
};

