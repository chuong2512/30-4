using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ActionQueue
{
    private readonly Queue<Func<Task>> _actionQueue  = new Queue<Func<Task>>();
    private          bool              _isProcessing = false;

    /// <summary>
    /// Thêm một action vào hàng đợi. Action này phải trả về một `Task`.
    /// </summary>
    public void EnqueueAction(Func<Task> action)
    {
        _actionQueue.Enqueue(action);

        // Nếu không có hành động nào đang chạy, bắt đầu xử lý.
        if (!_isProcessing)
        {
            ProcessActions();
        }
    }

    /// <summary>
    /// Xử lý các action trong hàng đợi.
    /// </summary>
    private async void ProcessActions()
    {
        _isProcessing = true;

        while (_actionQueue.Count > 0)
        {
            // Lấy hành động đầu tiên trong hàng đợi
            var action = _actionQueue.Dequeue();

            try
            {
                // Thực thi action (bất đồng bộ)
                await action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Có lỗi xảy ra trong action: {ex.Message}");
            }
        }

        _isProcessing = false; // Đặt trạng thái khi hoàn thành
    }
}