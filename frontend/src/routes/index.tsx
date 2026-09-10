import RootLayout from "@/components/layout/root-layout";
import DashboardPage from "@/components/pages/dashboard";
import DefectsPage from "@/components/pages/defectsPage";
import { createBrowserRouter } from "react-router";
import { paths } from "./navigation";

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
            }
        ],
    },
]);
