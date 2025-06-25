﻿// 1.Lấy phần tử từ HTML
const daysTag = document.querySelector(".days");
const currentDate = document.querySelector(".current-date");
const prevNextIcon = document.querySelectorAll(".icons span");

// 2.Lấy ngày mới, năm và tháng hiện tại
let date = new Date(),
    currYear = date.getFullYear(),
    currMonth = date.getMonth();

// 3.Tạo mảng lưu tên đầy đủ của tất cả các tháng
const months = [
    "Tháng 1",
    "Tháng 2",
    "Tháng 3",
    "Tháng 4",
    "Tháng 5",
    "Tháng 6",
    "Tháng 7",
    "Tháng 8",
    "Tháng 9",
    "Tháng 10",
    "Tháng 11",
    "Tháng 12"
];


let savedDates = [];
function GetDateByCount() {
    $.ajax({
        url: '/Admin/Appointment/GetDateByCount',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            console.log(data);
            savedDates = data;
            renderCalendar();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
GetDateByCount();

const renderCalendar = () => {
    let firstDayofMonth = new Date(currYear, currMonth, 1).getDay(); // Lấy ngày đầu tiên của tháng
    const lastDateofMonth = new Date(currYear, currMonth + 1, 0).getDate(); // Lấy ngày cuối cùng của tháng
    const lastDayofMonth = new Date(currYear, currMonth, lastDateofMonth).getDay(); // Lấy ngày cuối cùng của tháng (theo thứ)
    const lastDateofLastMonth = new Date(currYear, currMonth, 0).getDate(); // Lấy ngày cuối cùng của tháng trước

    // Điều chỉnh thứ bắt đầu từ Thứ Hai (0)
    if (firstDayofMonth === 0) {
        firstDayofMonth = 7;
    }

    let liTag = "";

    // Tạo các thẻ li cho những ngày cuối cùng của tháng trước
    for (let i = firstDayofMonth; i > 0; i--) {
        liTag += `<li class="none-click">${lastDateofLastMonth - i + 1}</li>`;
    }

    // Tạo các thẻ li cho tất cả các ngày trong tháng hiện tại
    for (let i = 1; i <= lastDateofMonth; i++) {
        let currentDate = new Date(currYear, currMonth, i + 1);
        // Kiểm tra xem ngày hiện tại có trong danh sách dữ liệu không
        let isActive = savedDates.includes(currentDate.toISOString().split('T')[0]) ? " none-click" : "";
        // Kiểm tra xem ngày hiện tại có phải là ngày hiện tại không
        isActive += (i === date.getDate() && currMonth === new Date().getMonth() && currYear === new Date().getFullYear()) ? " active" : "";
        // Kiểm tra xem ngày hiện tại có phải là thứ 7 hoặc chủ nhật không, nếu có thì thêm class "none-click"
        isActive += (currentDate.getDay() === 0 || currentDate.getDay() === 1) ? " none-click" : "";
        // Kiểm tra nếu ngày hiện tại cũ hơn ngày hiện tại, thêm class "text-danger"
        isActive += (currentDate < new Date()) ? " none-click" : "";
        // Kiểm tra nếu quá 1 tháng kể từ ngày hiện tại, thêm class "none-click"
        isActive += (currentDate.getTime() > new Date().getTime() + 30 * 24 * 60 * 60 * 1000) ? " none-click" : "";
        // Tạo thẻ li và thêm các lớp
        liTag += `<li id="date" class="${isActive}">${i}</li>`;
    }

    // Tạo các thẻ li cho những ngày đầu tiên của tháng sau
    for (let i = lastDayofMonth; i < 6; i++) {
        liTag += `<li class="none-click">${i - lastDayofMonth + 1}</li>`
    }
    // Hiển thị tháng và năm hiện tại
    currentDate.innerText = `${months[currMonth]}` + '-' + `${currYear}`;
    daysTag.innerHTML = liTag;

    addClickEventToDays(currMonth, currYear);
}
// Gọi hàm renderCalendar để hiển thị lịch
renderCalendar();

prevNextIcon.forEach(icon => { // getting prev and next icons
    icon.addEventListener("click", () => { // adding click event on both icons
        // if clicked icon is previous icon then decrement current month by 1 else increment it by 1
        currMonth = icon.id === "prev" ? currMonth - 1 : currMonth + 1;

        if (currMonth < 0 || currMonth > 11) { // if current month is less than 0 or greater than 11
            // creating a new date of current year & month and pass it as date value
            date = new Date(currYear, currMonth, new Date().getDate());
            currYear = date.getFullYear(); // updating current year with new date year
            currMonth = date.getMonth(); // updating current month with new date month
        } else {
            date = new Date(); // pass the current date as date value
        }
        renderCalendar(); // calling renderCalendar function
    });
});
// thêm sự kiện click
const formAppointment = document.querySelector('.formAppointment');
const close = document.querySelector('#close');
const datetime = document.querySelector('#datetime');

function addClickEventToDays(CurrMonth, CurrYear) {
    const calendarDays = document.querySelectorAll('#date');
    calendarDays.forEach(day => {
        day.addEventListener('click', (event) => {
            const selectedDay = parseInt(event.target.textContent);
            datetime.value = new Date(CurrYear, CurrMonth, selectedDay + 1).toISOString().slice(0, 10);
            formAppointment.style.display = 'block';
        });
    });
}

close.addEventListener('click', () => {
    formAppointment.style.display = 'none';  // Change display to 'block' on click
});


    