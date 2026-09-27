using System.Collections.Generic;

// Пользовательский компаратор равенства для NetworkedPlayerInfo, использующий ClientId
// Позволяет надёжно сравнивать объекты в коллекциях, даже если косметика, цвет и т.д. изменятся
public sealed class NetPlayerInfoCidComparer : IEqualityComparer<NetworkedPlayerInfo>
{
    // Проверка равенства двух игроков по их ClientId
    public bool Equals(NetworkedPlayerInfo data1, NetworkedPlayerInfo data2)
    {
        return data1.ClientId == data2.ClientId;
    }

    // Возвращает хеш-код на основе ClientId
    public int GetHashCode(NetworkedPlayerInfo data)
    {
        return data.ClientId;
    }
}
