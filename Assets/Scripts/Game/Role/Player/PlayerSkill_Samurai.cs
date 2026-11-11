using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSkill_Samurai : PlayerSkill
{
    [Header("剑气波")]
    public Projectile WaveObj;
    public float WaveDamageMutiply = 0.5f;
    public DmgBuffOnTouch ExpPillarObj;
    public float ExpDamage = 100f;

    IEnumerator SpawnWave(float time, Vector3 position, Vector3 dir)
    {
        yield return new WaitForSecondsRealtime(time);
        SpawnWave(position, dir);
    }

    void SpawnWave(Vector3 position, Vector3 dir)
    {
        if (WaveObj == null)
            return;
        Projectile waveInstance = Instantiate(WaveObj.gameObject, position, WaveObj.transform.rotation).GetComponent<Projectile>();
        waveInstance.transform.forward = dir;
        waveInstance.gameObject.SetActive(true);
        waveInstance.Init(roleController, roleController.AttackPower * WaveDamageMutiply);
    }

    void SpawnMutiWaveAround(Vector3 position, Vector3 middleDir, float angleRange, int waveCount)
    {
        float angle = angleRange / waveCount;
        for (int i = 0; i < waveCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0, -angleRange / 2 + i * angle, 0) * middleDir;
            StartCoroutine(SpawnWave(0.1f * i, position, dir));
        }
    }

    IEnumerator SpawnExp(float time, Vector3 position)
    {
        yield return new WaitForSeconds(time);
        var Instance=Instantiate(ExpPillarObj,position,ExpPillarObj.transform.rotation);
        Instance.gameObject.SetActive(true);
        Instance.Init(roleController,ExpDamage);
    }
    IEnumerator SpawnExp(float time, Transform targetTrans)
    {
        yield return new WaitForSeconds(time);
        var Instance=Instantiate(ExpPillarObj,targetTrans.position,ExpPillarObj.transform.rotation);
        Instance.gameObject.SetActive(true);
        Instance.Init(roleController,ExpDamage);
    }

    void SpawnExpUnderEnemy(float inteval,float maxCount)
    {
        List<RoleController> enemyList = BattleTool.GetRandomEnemys(roleController, 8);
        if (enemyList.Count < maxCount)
        {
            for (int i = 0; i < maxCount - enemyList.Count;i++)
            {
                enemyList.Insert(  Random.Range(0,enemyList.Count),null);
            }
        }

        for (int i = 0; i < enemyList.Count; i++)
        {
            if (!enemyList[i])
            {
                StartCoroutine(SpawnExp(inteval * i, roleController.transform.position+new Vector3(Random.Range(-6,6),0,Random.Range(-6,6))));
            }
            else
            {
                StartCoroutine(SpawnExp(inteval * i, enemyList[i].transform));
            }
        }
    }

    protected override void AnimEvent(GameObject go, string eventName)
    {
        if (go != roleController.Animator.gameObject)
        {
            return;
        }

        if (eventName.Contains("Katana_SpawnWave_"))
        {
            eventName = eventName.Replace("Katana_SpawnWave_", "");
            float time = float.Parse(eventName);
            
            StartCoroutine(SpawnWave(time > 0 ? time : 0, WaveObj.transform.position, WaveObj.transform.forward));
            if (roleColorLevel >= 2)
            {
                StartCoroutine(SpawnWave((time > 0 ? time : 0)+0.1f, WaveObj.transform.position+roleController.Animator.transform.right*0.5f,  Quaternion.AngleAxis(40,Vector3.up) *WaveObj.transform.forward));
                StartCoroutine(SpawnWave((time > 0 ? time: 0)+0.1f, WaveObj.transform.position-roleController.Animator.transform.right*0.5f,  Quaternion.AngleAxis(-40,Vector3.up) *WaveObj.transform.forward));
            }
        }
        if (eventName.Contains("Katana_SpawnWaveCharge"))
        {
            StartCoroutine(SpawnWave(0, WaveObj.transform.position,  WaveObj.transform.forward));
            StartCoroutine(SpawnWave(0.2f, WaveObj.transform.position,  Quaternion.AngleAxis(20,Vector3.up) *WaveObj.transform.forward));
            StartCoroutine(SpawnWave(0.2f , WaveObj.transform.position,  Quaternion.AngleAxis(-20,Vector3.up) *WaveObj.transform.forward));
            if (roleColorLevel >=2)
            {
                StartCoroutine(SpawnWave(0.3f, WaveObj.transform.position,  Quaternion.AngleAxis(40,Vector3.up) *WaveObj.transform.forward));
                StartCoroutine(SpawnWave(0.3f , WaveObj.transform.position,  Quaternion.AngleAxis(-40,Vector3.up) *WaveObj.transform.forward));
            }
        }
        else if (eventName.Contains("Katana_SpawnExpFwd"))
        {
            
           StartCoroutine(SpawnExp(0,roleController.transform.position+roleController.Animator.transform.forward));
        }

        else if (eventName.Contains("Katana_SpawnMutiWaveAround_"))
        {
            eventName = eventName.Replace("Katana_SpawnMutiWaveAround_", "");
            string[] strArray = eventName.Split('_');
            float angleRange = float.Parse(strArray[0]);
            int waveCount = int.Parse(strArray[1]);
            if (WaveObj != null)
                 SpawnMutiWaveAround(WaveObj.transform.position, roleController.Animator.transform.forward, angleRange, waveCount);
            if (roleColorLevel >=3)
            {
                SpawnExpUnderEnemy(0.24f, 8);
            }
        }
    }
    
}

