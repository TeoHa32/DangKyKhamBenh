// code xoay icon khi collapse
$(document).ready(function () {
    // Lớp theo dõi trạng thái của collapse
    var collapseStates = {};

    // Xoay icon khi nhấn vào
    $('.nav-link').click(function () {
        $(this).children('.collapse-toggle').toggleClass('rotated');
    });

    // Theo dõi trạng thái của collapse
    $('.collapse').on('show.bs.collapse', function () {
        collapseStates[this.id] = true;
    });

    // Xoay icon trở về ban đầu khi mở collapse khác
    $('.collapse').on('shown.bs.collapse', function () {
        $('.collapse-toggle').each(function () {
            if (!collapseStates[this.parentNode.id]) {
                $(this).removeClass('rotated');
            }
        });
    });
});