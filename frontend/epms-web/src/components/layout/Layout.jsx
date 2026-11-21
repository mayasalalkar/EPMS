import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Topbar from './Topbar';
import Footer from './Footer';

function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="app-shell">
      <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      {sidebarOpen && <div className="app-backdrop d-lg-none" onClick={() => setSidebarOpen(false)}></div>}

      <div className="app-main">
        <Topbar onMenuClick={() => setSidebarOpen(true)} />
        <main className="flex-grow-1 p-3 p-lg-4">
          <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
}

export default Layout;
