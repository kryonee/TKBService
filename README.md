# 📅 TKBService - Hệ thống sắp xếp thời khoá biểu

## 🚀 Mục tiêu

Ứng dụng sắp xếp thời khoá biểu tự động từ dữ liệu đầu vào gọi từ API. Kết quả được xuất ra:


---


## 📂 Cấu trúc thư mục

```
TKBService/
├── Controllers/
│   └── ScheduleController.cs       # Điều phối xử lý
├── Models/
│   └── ScheduleItem.cs            # Cấu trúc dữ liệu
├── Services/
│   ├── ScheduleService.cs         # Logic sắp xếp
├── Program.cs                     # Điểm bắt đầu
```

---

## 📤 Kết quả đầu ra

* `scheduled_by_class.json`: Thời khoá biểu nhóm theo lớp
---

## ⚙️ Tuỳ chỉnh nâng cao

* Số lượng giáo viên: trong `ScheduleService`
* Số môn mỗi GV có thể dạy
* Logic tránh dồn ca, ghi lại log những lớp không sắp xếp được

---


