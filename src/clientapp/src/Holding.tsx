import logo from '../src/assets/node_fx.jpg'
import './App.css'

function Holding() {
  return (
    <div className="App">
      <header className="App-header">
        <p className="coming-soon-text">Vertical Slice</p>
        <img src={logo} className="App-logo" alt="logo" />
        <p className="coming-soon-text">Coming Soon</p>
      </header>
    </div>
  )
}

export default Holding
