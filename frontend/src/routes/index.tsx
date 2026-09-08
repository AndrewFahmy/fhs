import RootLayout from "@/components/layout/rootLayout";
import DashboardPage from "@/components/pages/dashboard";
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
        ],
    },
]);
