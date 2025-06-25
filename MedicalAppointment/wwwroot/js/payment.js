let MY_BANK = {
    BANK_ID: "BIDV",
    ACCOUNT_NO: "5730556448"
}
document.addEventListener('DOMContentLoaded', () => {
    const btn = document.querySelector(`.btn-thanhtoan`);
    const paid_content = document.querySelector("#paid_content");
    const paid_price = document.querySelector("#paid_price");
    const img_qr = document.querySelector(".img_qr");
    const price = document.querySelector('.price').innerText;
    const department = document.querySelector('.department').innerText;
    const normalizedDepartment = removeVietnameseDiacritics(department);
    const username = document.querySelector('.username').innerText;
    btn.addEventListener("click", () => {
        var overlay = document.getElementById("overlay");
        var myDiv = document.querySelector(".b");
        if (myDiv.style.display === "none") {
            myDiv.style.display = "block";
            overlay.style.display = "block";
        } else {
            myDiv.style.display = "none";
            overlay.style.display = "none";
        }
        overlay.addEventListener('click', () => {
            myDiv.style.display = "none";
            overlay.style.display = "none";
        })
        isSuccess = false;
        const paidContent = username.concat("", normalizedDepartment);
        const paidPrice = price;
        let qr = `https://img.vietqr.io/image/${MY_BANK.BANK_ID}-${MY_BANK.ACCOUNT_NO}-qr_only.png?amount=${paidPrice}&addInfo=${paidContent}`;
        img_qr.src = qr;
        paid_content.innerHTML = paidContent;
        paid_price.innerHTML = paidPrice;
        setTimeout(() => {
            setInterval(() => {
                checkPaid(paidPrice, paidContent);
                
            }, 1000);
        }, 2000);
    })
});
function removeVietnameseDiacritics(str) {
    str = str.toLowerCase();
    str = str.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
    str = str.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
    str = str.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
    str = str.replace(/[íìỉĩị]/g, 'i');
    str = str.replace(/[úùủũụưứừửữự]/g, 'u');
    str = str.replace(/[ýỳỷỹỵ]/g, 'y');
    str = str.replace(/đ/g, 'd');
    return str;
}
function showAlert() {
    Swal.fire({
        title: 'Thông Báo',
        text: 'Thanh toán thành công',
        icon: 'info',
        confirmButtonText: 'OK'
    }).then((result) => {
        
        if (result.isConfirmed) {
           
            window.location.assign("/Home/Index");
        }
    });
}
let isSuccess = false;
async function checkPaid(price, content) {
    if (isSuccess == true) {

    }
    else {
        try {
            const response = await fetch("https://script.google.com/macros/s/AKfycbznZN8s8oHyIEf5IFpFhZk_EDHlmNGRcBKVbFQz2OaPBr4oZaOPEekDw-vSWkE9JUPPYw/exec");
            const data = await response.json();
            const lastPaid = data.data[data.data.length - 1];
            const lastPrice = lastPaid["Giá trị"];
            const lastContent = lastPaid["Mô tả"];
            console.log(lastContent);
            console.log(lastPrice);
            console.log(price);
            console.log(lastContent);
            console.log(content);
            if (lastPrice >= price && lastContent.includes(content)) {
                showAlert();
                isSuccess = true;
            } else {
                console.log("Không thành công");
            }
        } catch {
            console.error("Lỗi");
        }
    }
}