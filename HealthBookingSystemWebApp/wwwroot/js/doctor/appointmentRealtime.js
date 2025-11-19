const connection = new signalR.HubConnectionBuilder()
    .withUrl("/appointmentHub")
    .build();

connection.start().catch(err => console.error(err));

connection.on("ReceiveAppointmentUpdate", function (appointmentId, status) {

    // Khi cập nhật status, reload cả 5 tab:
    reloadTab("#todaySchedule", "/Doctor/ReloadDashboardToday");
    reloadTab("#todayAppointments", "/Doctor/ReloadToday");
    reloadTab("#upcomingAppointments", "/Doctor/ReloadUpcoming");
    reloadTab("#completedAppointments", "/Doctor/ReloadCompleted");
    reloadTab("#cancelledAppointments", "/Doctor/ReloadCancelled");
});

function reloadTab(containerSelector, url) {
    $.get(url, function (html) {
        $(containerSelector).html(html);
    });
}
