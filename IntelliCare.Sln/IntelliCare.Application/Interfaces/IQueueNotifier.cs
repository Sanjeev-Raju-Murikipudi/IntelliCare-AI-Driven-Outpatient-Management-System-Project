using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.Interfaces
{
    public interface IQueueNotifier
    {
        Task NotifyQueueUpdateAsync(int doctorId);
    }

}
