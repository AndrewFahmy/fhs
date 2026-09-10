import RootLayout from "@/components/layout/root-layout";
import DashboardPage from "@/components/pages/dashboard";
import DefectsPage from "@/components/pages/defectsPage";
import DefectDetailPage from "@/components/pages/defectDetailPage";
import { createBrowserRouter } from "react-router";
import { paths } from "@/routes/navigation";
import NewDefectPage from "@/components/pages/newDefectPage";

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
                element: <DefectDetailPage />,
            },
            {
                path: paths.newDefect,
                element: <NewDefectPage />,
            },
        ],
    },
]);
