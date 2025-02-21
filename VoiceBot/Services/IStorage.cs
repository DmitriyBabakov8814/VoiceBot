using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceBot.Models;
namespace VoiceBot.Services
{
    public interface IStorage
    {
        Session GetSession(long chatId);
    }
}
