using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class MoCapMap
{
    public Transform self;
    public Vector3 rotationOffset;
    public Vector3 positionOffset;

    public void Map(Quaternion rotation, Vector3 position, bool enablePos)
    {
        if (enablePos) {
            self.position = position;
            self.position = self.TransformPoint(positionOffset);
        }
        self.rotation = rotation * Quaternion.Euler(rotationOffset);
    }


}

public class SharedMemoryToModel : SharedMemoryReader
{
    
    public MoCapMap[] objects = new MoCapMap[16];
    public Transform body;
    [Range(-180f, 180f)]
    public float x; //110
    [Range(-180f, 180f)]
    public float y;
    [Range(-180f, 180f)]
    public float z; //180

    // set as Transform
    public override void OutputData(float[] buffer)
    {
        for (int i = 0; i < objects.Length && i * 7 + 6 < buffer.Length; i++)
        {
            int offset = i * 7;
            int[] white_list = {0, 1, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14};
            bool execute = false;
            for (int j = 0; j < white_list.Length; j++){
                if (i == white_list[j]){
                    execute = true;
                }
            }
            if (execute) {
                Quaternion rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]); // (UE4.rot.x, -UE4.rot.z, UE4.rot.y, UE4.rot.w)
                Vector3 position = new Vector3(buffer[offset + 4], buffer[offset + 6], -buffer[offset + 5]) / 100.0f; // (UE4.pos.x, -UE4.pos.z, UE4.pos.y) / 100
                objects[i].Map(rotation, position, false);
            }
        }
        Quaternion rot = new Quaternion(buffer[0], buffer[2], -buffer[1], buffer[3]); // (UE4.rot.x, -UE4.rot.z, UE4.rot.y, UE4.rot.w)
        Vector3 pos = new Vector3(buffer[4], buffer[6], -buffer[5]) / 100.0f; // (UE4.pos.x, -UE4.pos.z, UE4.pos.y) / 100
        body.position = pos;
        //Vector3 offsetTest = new Vector3(x, y, z);
        Vector3 bodyOffset = new Vector3(110, 0, 180);
        body.rotation = rot * Quaternion.Euler(bodyOffset);

        //body & pelvis

        //chest
        

        //left arm
        //shoulderL
        //Vector3 offsetRotation = new Vector3(270, 0, 180);
        //int offset = 4*7;
        //Quaternion rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //Vector3 position = new Vector3(0, 0, 0);
        //objects[offset].Map(rotation, position, false);
        //Vector3 offsetRotation = new Vector3(180, 0, 180);
        //shoulderL.transform.rotation = rotation * Quaternion.Euler(offsetRotation);
        //elbowL
        //offsetRotation = new Vector3(270, 0, 180);
        //offset = 5*7;
        //rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //offsetRotation = new Vector3(180, 0, 90);
        //objects[offset].Map(rotation, position, false);
        //elbowL.transform.rotation = rotation * Quaternion.Euler(offsetRotation);
        //handL
        //offsetRotation = new Vector3(270, 0, 180);
        //offset = 6*7;
        //rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //objects[offset].Map(rotation, position, false);
        //offsetRotation = new Vector3(-90, 0, 90);
        //handL.transform.rotation = rotation * Quaternion.Euler(offsetRotation);

        //right arm
        //shoulderR
        //offsetRotation = new Vector3(270, 0, 0);
        //offset = 7*7;
        //rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //objects[offset].Map(rotation, position, false);
        //offsetRotation = new Vector3(180, 0, 180);
        //shoulderR.transform.rotation = rotation * Quaternion.Euler(offsetRotation);
        //elbowR
        //offsetRotation = new Vector3(270, 0, 0);
        //offset = 8*7;
        //rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //offsetRotation = new Vector3(90, 0, 180);
        //objects[offset].Map(rotation, position, false);
        //elbowR.transform.rotation = rotation * Quaternion.Euler(offsetRotation);
        //handR
        //offsetRotation = new Vector3(270, 0, 0);
        //offset = 9*7;
        //rotation = new Quaternion(buffer[offset], buffer[offset + 2], -buffer[offset + 1], buffer[offset + 3]);
        //offsetRotation = new Vector3(90, 180, -90);
        //objects[offset].Map(rotation, position, false);
        //handR.transform.rotation = rotation * Quaternion.Euler(offsetRotation);

        //left leg
        //hipL
        //offsetRotation = new Vector3(270, 0, 0);
        //kneeL
        //offsetRotation = new Vector3(270, 0, 0);

        //right leg
        //hipR
        //offsetRotation = new Vector3(270, 0, 180);
        //kneeR
        //offsetRotation = new Vector3(270, 0, 180);
        
    }
}
