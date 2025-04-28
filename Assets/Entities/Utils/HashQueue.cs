namespace HoleBox.Utils
{
    using System.Collections.Generic;

    public class HashQueue<T>
    {
        private Queue<T>   queue = new Queue<T>();
        private HashSet<T> set   = new HashSet<T>();

        public int Count => queue.Count;

        // Thêm phần tử nếu chưa tồn tại
        public bool Enqueue(T item)
        {
            if (set.Contains(item))
                return false; // Đã có rồi, không thêm

            queue.Enqueue(item);
            set.Add(item);
            return true;
        }

        // Lấy phần tử ra (và xóa khỏi hash)
        public T Dequeue()
        {
            var item = queue.Dequeue();
            set.Remove(item);
            return item;
        }

        // Xem phần tử đầu tiên mà không remove
        public T Peek() { return queue.Peek(); }

        // Kiểm tra phần tử có trong queue không
        public bool Contains(T item) { return set.Contains(item); }

        // Xóa tất cả
        public void Clear()
        {
            queue.Clear();
            set.Clear();
        }
    }
}