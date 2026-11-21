import { lazy, Suspense } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ToastProvider } from './context/ToastContext';
import GuestRoute from './components/GuestRoute';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/layout/Layout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import Loader from './components/Loader';
import EmployeesPage from './pages/EmployeesPage';
import ProjectsPage from './pages/ProjectsPage';
import MyProjectsPage from './pages/MyProjectsPage';
import NotFoundPage from './pages/NotFoundPage';
import { ROLES } from './utils/constants';

const DashboardPage = lazy(() => import('./pages/DashboardPage'));

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <Routes>
            <Route element={<GuestRoute />}>
              <Route path="login" element={<LoginPage />} />
              <Route path="register" element={<RegisterPage />} />
            </Route>
            <Route element={<ProtectedRoute />}>
              <Route element={<Layout />}>
                <Route index element={<Suspense fallback={<Loader />}><DashboardPage /></Suspense>} />
                <Route element={<ProtectedRoute roles={[ROLES.admin]} />}>
                  <Route path="employees" element={<EmployeesPage />} />
                  <Route path="projects" element={<ProjectsPage />} />
                </Route>
                <Route element={<ProtectedRoute roles={[ROLES.employee]} />}>
                  <Route path="my-projects" element={<MyProjectsPage />} />
                </Route>
              </Route>
            </Route>
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
