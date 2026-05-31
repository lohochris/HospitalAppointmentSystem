# 🏥 ESI TRIAGE VISUAL QUICK REFERENCE

## Emergency Severity Index (ESI) Color System

---

## 🔴 **LEVEL 1 - CRITICAL** (Immediate Life-Threatening)

**Visual Indicator**: Soft Red Background (255, 200, 200) + **BOLD Font**

### Clinical Triggers (ANY ONE triggers Level 1):
- ❌ **SpO2 < 90%** - Severe Hypoxemia
- ❌ **Systolic BP ≥ 180 mmHg** - Hypertensive Crisis
- ❌ **Diastolic BP ≥ 120 mmHg** - Hypertensive Emergency
- ❌ **Heart Rate > 130 bpm** - Severe Tachycardia

### Queue Behavior:
- **AUTO-SORTED TO TOP** of doctor's queue
- **BOLD FONT** for immediate visual attention
- Selection color: Darker red for visibility

### Treatment Timeframe:
- **⚡ IMMEDIATE** (<5 minutes)

### Example Patient:
```
┌────────────────────────────────────────────────────────────┐
│ [🔴 BOLD] PAT-000001 | John Doe | SpO2: 88% | HR: 75 bpm  │
│           Blood Pressure: 120/80 mmHg                      │
│           Status: CRITICAL - Severe Hypoxemia              │
└────────────────────────────────────────────────────────────┘
```

---

## 🟡 **LEVEL 2 - URGENT** (High-Risk Deterioration)

**Visual Indicator**: Soft Amber Background (255, 235, 180) + Regular Font

### Clinical Triggers (ANY ONE triggers Level 2):
- ⚠️ **SpO2 90-94%** - Moderate Hypoxemia
- ⚠️ **Systolic BP 140-179 mmHg** - Stage 2 Hypertension
- ⚠️ **Heart Rate 100-129 bpm** - Moderate Tachycardia

### Queue Behavior:
- **Sorted BELOW Level 1**, **ABOVE Level 3**
- Regular font
- Selection color: Darker amber

### Treatment Timeframe:
- **🚑 URGENT** (<15 minutes)

### Example Patient:
```
┌────────────────────────────────────────────────────────────┐
│ [🟡] PAT-000004 | Alice Smith | SpO2: 92% | HR: 85 bpm    │
│      Blood Pressure: 135/88 mmHg                           │
│      Status: URGENT - Moderate Hypoxemia                   │
└────────────────────────────────────────────────────────────┘
```

---

## 🟢 **LEVEL 3 - MODERATE** (Stable, Requires Evaluation)

**Visual Indicator**: Soft Yellow Background (255, 255, 200) + Regular Font

### Clinical Criteria:
- ✅ **SpO2 ≥ 95%** - Normal Oxygen Saturation
- ✅ **Systolic BP 90-139 mmHg** - Normal Range
- ✅ **Diastolic BP 60-89 mmHg** - Normal Range
- ✅ **Heart Rate 60-99 bpm** - Normal Range

### Queue Behavior:
- **Sorted at BOTTOM** of queue
- Regular font
- Selection color: Darker yellow

### Treatment Timeframe:
- **⏱️ MODERATE** (30-60 minutes)

### Example Patient:
```
┌────────────────────────────────────────────────────────────┐
│ [🟢] PAT-000006 | Bob Johnson | SpO2: 98% | HR: 75 bpm    │
│      Blood Pressure: 120/80 mmHg                           │
│      Status: STABLE - All vitals normal                    │
└────────────────────────────────────────────────────────────┘
```

---

## ⚪ **INCOMPLETE DATA** (Missing Vitals)

**Visual Indicator**: Soft Gray Background (240, 240, 240) + Regular Font

### Trigger:
- ❓ **Missing vitals record** in InpatientVitals table
- ❓ **Null/empty vitals** (no recent measurement)

### Queue Behavior:
- **Default to Level 3** (Moderate) - Safety fallback
- Regular font
- Selection color: Darker gray

### System Response:
- **No crash** - Defensive null handling
- Audit log: "WARNING - Incomplete vitals data"
- Prompts staff to record vitals

### Example Patient:
```
┌────────────────────────────────────────────────────────────┐
│ [⚪] PAT-000008 | Carol White | Vitals: NOT RECORDED       │
│     Status: INCOMPLETE DATA - Please measure vitals        │
└────────────────────────────────────────────────────────────┘
```

---

## ❌ **INVALID VITALS** (Physiologically Impossible)

**Visual Indicator**: Soft Gray Background (240, 240, 240) + Regular Font

### Trigger:
- ❌ **SpO2 > 100%** (impossible oxygen saturation)
- ❌ **SpO2 < 0%** (impossible negative value)
- ❌ **Heart Rate < 20 or > 250 bpm** (physiological extremes)
- ❌ **Systolic BP ≤ Diastolic BP** (measurement error)

### Queue Behavior:
- **Default to Level 3** (Moderate) - Safety fallback
- Regular font
- Audit log: "ERROR - Physiologically invalid vitals"

### System Response:
- **Rejects invalid data**
- Prompts staff to re-measure vitals
- Prevents corruption of triage calculations

---

## 📊 COMPLETE QUEUE VISUALIZATION

### Example Doctor Dashboard (Auto-Sorted by ESI Level):

```
┌───────────────────────────────────────────────────────────────────────────┐
│                   DR. JAMES OKAFOR - APPOINTMENT QUEUE                    │
│                        (Auto-Sorted by ESI Level)                         │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ 🔴 CRITICAL (ESI LEVEL 1) - Immediate Intervention Required              │
│ ┌───────────────────────────────────────────────────────────────────┐   │
│ │ [BOLD] PAT-001 | Severe Hypoxemia      | SpO2: 88%  | 09:00 AM   │   │
│ │ [BOLD] PAT-005 | Hypertensive Crisis   | BP: 185/125 | 09:30 AM   │   │
│ │ [BOLD] PAT-003 | Severe Tachycardia    | HR: 145 bpm | 10:00 AM   │   │
│ └───────────────────────────────────────────────────────────────────┘   │
│                                                                           │
│ 🟡 URGENT (ESI LEVEL 2) - High-Risk Deterioration                        │
│ ┌───────────────────────────────────────────────────────────────────┐   │
│ │ PAT-004 | Moderate Hypoxemia    | SpO2: 92%  | 10:30 AM           │   │
│ │ PAT-007 | Stage 2 HTN           | BP: 155/95 | 11:00 AM           │   │
│ └───────────────────────────────────────────────────────────────────┘   │
│                                                                           │
│ 🟢 STABLE (ESI LEVEL 3) - Routine Evaluation                             │
│ ┌───────────────────────────────────────────────────────────────────┐   │
│ │ PAT-006 | Normal Vitals         | SpO2: 98%  | 11:30 AM           │   │
│ │ PAT-002 | Normal Vitals         | SpO2: 99%  | 01:00 PM           │   │
│ │ PAT-008 | Incomplete Data       | No Vitals  | 01:30 PM           │   │
│ └───────────────────────────────────────────────────────────────────┘   │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

---

## 🎨 COLOR PALETTE SPECIFICATIONS

| ESI Level | Name | RGB | Hex Code | Use Case |
|-----------|------|-----|----------|----------|
| **1** | CRITICAL | (255, 200, 200) | #FFC8C8 | Life-threatening emergencies |
| **2** | URGENT | (255, 235, 180) | #FFEBB4 | High-risk deterioration |
| **3** | MODERATE | (255, 255, 200) | #FFFFC8 | Stable, requires evaluation |
| **4** | LOW | (200, 255, 200) | #C8FFC8 | Non-urgent, minimal intervention |
| **5** | MINIMAL | (200, 230, 255) | #C8E6FF | Minor issues, long wait tolerable |
| **N/A** | INCOMPLETE | (240, 240, 240) | #F0F0F0 | Missing or invalid vitals data |

---

## ⚡ QUICK TROUBLESHOOTING

### ❌ Colors Not Appearing?
1. ✅ Check `System.Data` import in FormMain.vb
2. ✅ Verify `AddHandler dgvDashboardData.CellFormatting` exists
3. ✅ Rebuild solution (Ctrl+Shift+B)

### ❌ All Patients Showing Gray?
1. ✅ Insert vitals using `ESI_TRIAGE_TEST_SCRIPT.sql`
2. ✅ Verify `InpatientVitals.PatientID` matches `Appointments.PatientID`
3. ✅ Check HospitalErrors.log for "WARNING - Incomplete vitals data"

### ❌ Critical Patients Not at Top?
1. ✅ Verify `GetDoctorAppointments()` contains auto-sort logic
2. ✅ Check log: "Retrieved X appointments with ESI triage color-coding"
3. ✅ Ensure ESI calculation runs BEFORE DataView.Sort

---

## 📞 TESTING CHECKLIST

- [ ] Login as doctor: `dr_james_okafor` / `Doctor@123`
- [ ] Navigate to main dashboard (auto-loads on login)
- [ ] Verify critical patients (red background) appear at TOP
- [ ] Verify urgent patients (amber background) appear in MIDDLE
- [ ] Verify stable patients (yellow background) appear at BOTTOM
- [ ] Select a row and verify darker selection color
- [ ] Check HospitalErrors.log for audit trail entries
- [ ] Run `ESI_TRIAGE_TEST_SCRIPT.sql` for comprehensive test data

---

**Your ESI visual triage system is now live! 🏥🚨**

**Quick Reference Version**: 1.0  
**Last Updated**: January 2026  
**Status**: ✅ Production-Ready
