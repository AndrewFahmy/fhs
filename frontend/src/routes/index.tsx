import RootLayout from "@/components/layout/root-layout";
import DashboardPage from "@/components/pages/dashboard-page";
import DefectsPage from "@/components/pages/defects-page";
import DefectDetailsPage from "@/components/pages/defect-details-page";
import { createBrowserRouter } from "react-router";
import { paths } from "@/routes/navigation";
import NewDefectPage from "@/components/pages/new-defect-page";
import EscapesPage from "@/components/pages/escapes-page";
import EscapeDetailsPage from "@/components/pages/escape-details-page";
import NewEscapePage from "@/components/pages/new-escape-page";
import StationsPage from "@/components/pages/stations-page";
import ErrorCodesPage from "@/components/pages/error-codes-page";
import CustomersPage from "@/components/pages/customers-page";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <RootLayout />,
        children: [
            {
                path: paths.dashboard,
                element: <DashboardPage />,
                index: true,
            },
            {
                path: paths.defects,
                element: <DefectsPage />,
            },
            {
                path: paths.defectDetail,
                element: <DefectDetailsPage />,
            },
            {
                path: paths.newDefect,
                element: <NewDefectPage />,
            },
            {
                path: paths.escapes,
                element: <EscapesPage />,
            },
            {
                path: paths.escapeDetail,
                element: <EscapeDetailsPage />,
            },
            {
                path: paths.newEscape,
                element: <NewEscapePage />,
            },
            {
                path: paths.stations,
                element: <StationsPage />,
            },
            {
                path: paths.errorCodes,
                element: <ErrorCodesPage />,
            },
            {
                path: paths.customers,
                element: <CustomersPage />,
            },
        ],
    },
]);
